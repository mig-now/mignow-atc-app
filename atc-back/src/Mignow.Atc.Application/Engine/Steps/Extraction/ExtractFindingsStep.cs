using System.Globalization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Ports.Sap.Extraction;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Extraction;

namespace Mignow.Atc.Application.Engine.Steps.Extraction;

/// <summary>
/// Segundo passo de <c>Extraction</c> (<c>extrair.ps1</c>, seções 2 a 4): findings pelo ADT, complementos por SQL, montagem das
/// colunas, gravação de <see cref="AtcExtraction"/> + <see cref="AtcFinding"/> em lotes, contagens P1/P2/P3 (total e sem
/// baseline) e variação contra a extração anterior. Idempotente: a extração só vale depois do último lote
/// (<see cref="AtcExtraction.IsComplete"/>); a que ficou pela metade é apagada e refeita.
/// </summary>
public sealed class ExtractFindingsStep(IAtcExtractionReader reader) : IRunStep
{
    private const int ChunkSize = 1000;

    public string Code => ExtractionStepCodes.Extract;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var state = Json.Read<ExtractionRunResult?>(ctx.Run.ResultJson, null);
        if (state?.Latest is not { } latest)
            return StepOutcome.Stop("Extraction.NoLatest");
        if (state.Situation is ExtractionRunResult.AlreadyExtracted or ExtractionRunResult.Extracted)
        {
            await ctx.ReportProgressAsync(1, 1);
            return StepOutcome.Done;
        }

        var started = ctx.Clock.UtcNow;
        var language = string.IsNullOrWhiteSpace(ctx.SapSystem.Language) ? "EN" : ctx.SapSystem.Language.Trim().ToUpperInvariant();
        await ctx.ReportProgressAsync(0, 4);

        // 1) findings via ADT
        var raw = await reader.ReadFindingsAsync(ctx.SapConnection, latest.DisplayId, language, ctx.Ct);
        if (raw.IsFailure)
            return Fail(raw.Error);
        await ctx.LogAsync(EventLevel.Info, "Extraction.FindingsRead", Json.Args(("count", raw.Value.Rows.Count)));
        await ctx.ReportProgressAsync(1, 4);
        if (await ctx.IsCancellationRequestedAsync())
            return StepOutcome.Cancel;

        // 2) complementos (item, baseline, TADIR, nota, textos de domínio)
        var request = new AtcComplementRequest(
            ExtractionTableBuilder.ComplementKeys(raw.Value, "item_id"), ExtractionTableBuilder.ComplementKeys(raw.Value, "obj_name"),
            ExtractionTableBuilder.ComplementKeys(raw.Value, "tag_NOTE"), language);
        var complements = await reader.ReadComplementsAsync(ctx.SapConnection, request, ctx.Ct);
        if (complements.IsFailure)
            return Fail(complements.Error);
        if (complements.Value.MissingTables is { Count: > 0 } missing)
            await ctx.LogAsync(EventLevel.Warning, "Extraction.OptionalTablesMissing", Json.Args(("tables", string.Join(", ", missing))));
        await ctx.ReportProgressAsync(2, 4);
        if (await ctx.IsCancellationRequestedAsync())
            return StepOutcome.Cancel;

        // 3) tabela, contagens, variação
        var table = ExtractionTableBuilder.Build(raw.Value, complements.Value);
        var counts = ExtractionCounts.Of(table.Rows);
        var quality = ExtractionQuality.Of(table.Rows);
        var previous = await ctx.WithDbAsync(() => ExtractionQueries.FindPreviousAsync(ctx.Store, ctx.SapSystem.Id, latest.DisplayId, CancellationToken.None));
        var variation = previous is null ? null : ExtractionVariation.Compute(counts, previous);
        await ctx.ReportProgressAsync(3, 4);

        // 4) gravação em lotes
        await DiscardIncompleteAsync(ctx, latest.DisplayId);
        var extraction = AtcExtraction.Create(ctx.SapSystem.Id, latest.Series, latest.DisplayId, latest.Title, latest.ExecutedAtUtc, started);
        extraction.SetColumns(Json.Write(table.Columns));
        extraction.SetExecution(Json.Write(new
        {
            latest.DisplayId, latest.ScheduledBy, latest.CheckProfile, latest.Configuration, latest.Prio1, latest.Prio2, latest.Prio3, latest.Prio4,
            latest.PlanningErrors, Language = language, latest.InProgress, Quality = quality, Columns = table.Columns.Count,
        }));
        await ctx.MutateAsync(() => ctx.Store.Add(extraction));
        await ctx.SaveAsync();

        var chunks = (table.Rows.Count + ChunkSize - 1) / ChunkSize;
        var total = 3 + Math.Max(chunks, 1);
        for (var c = 0; c < chunks; c++)
        {
            var from = c * ChunkSize;
            await ctx.MutateAsync(() =>
            {
                for (var i = from; i < Math.Min(from + ChunkSize, table.Rows.Count); i++)
                    ctx.Store.Add(ToFinding(extraction.Id, i + 1, table.Rows[i], table.Columns));
            });
            await ctx.SaveAsync();
            await ctx.ReportProgressAsync(3 + c + 1, total);
            if (c < chunks - 1 && await ctx.IsCancellationRequestedAsync())
                return StepOutcome.Cancel;
        }

        await ctx.MutateAsync(() =>
        {
            extraction.SetCounts(counts.Total, counts.InBaseline, counts.P1Total, counts.P2Total, counts.P3Total, counts.P1New, counts.P2New, counts.P3New);
            extraction.SetVariation(variation is null ? null : Json.Write(variation));
            extraction.MarkComplete();
        });
        await ctx.SaveAsync();
        await ctx.ReportProgressAsync(total, total);

        await ctx.SetResultAsync(new ExtractionRunResult(ExtractionRunResult.Extracted, latest, extraction.Guid, counts.Total, counts.InBaseline, counts.New,
            new PriorityCounts(counts.P1Total, counts.P2Total, counts.P3Total), new PriorityCounts(counts.P1New, counts.P2New, counts.P3New), variation, quality,
            table.Columns.Count));
        var args = Json.Args(("total", counts.Total), ("new", counts.New), ("inBaseline", counts.InBaseline),
            ("seconds", ((int)(ctx.Clock.UtcNow - started).TotalSeconds).ToString(CultureInfo.InvariantCulture)));
        await ctx.LogAsync(EventLevel.Info, "Extraction.Extracted", args);
        return StepOutcome.Info("Extraction.Extracted", args);
    }

    private static StepOutcome Fail(Error error) => StepOutcome.Stop(error.Code, Json.Args(("message", error.Message)));

    private static AtcFinding ToFinding(int extractionId, int seq, IReadOnlyDictionary<string, string> row, IReadOnlyList<string> columns)
    {
        string V(string c) => ExtractionTableBuilder.Value(row, c);
        var name = V("obj_name");
        return AtcFinding.Create(extractionId, seq, int.TryParse(V("priority"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : null,
            V("checkTitle"), V("messageTitle"), name.Length > 0 ? name : "(unknown)", V("obj_type"), V("obj_packageName"), V("tag_REF_OBJ_NAME"),
            V("tag_REF_OBJ_TYPE"), V("tag_NOTE"), V("quickfixInfo"), !ExtractionTableBuilder.IsNew(row),
            Json.Write(columns.ToDictionary(c => c, V, StringComparer.Ordinal)));
    }

    /// <summary>Apaga extrações desta execução que ficaram pela metade (queda no meio da gravação) com os findings já gravados.</summary>
    private static async Task DiscardIncompleteAsync(RunContext ctx, string displayId)
    {
        var systemId = ctx.SapSystem.Id;
        var leftovers = await ctx.WithDbAsync(() => ctx.Store.ListAsync(
            ctx.Store.Query<AtcExtraction>().Where(e => e.SapSystemId == systemId && e.DisplayId == displayId && !e.IsComplete), CancellationToken.None));
        foreach (var e in leftovers)
        {
            var id = e.Id;
            while (true)
            {
                var findings = await ctx.WithDbAsync(() => ctx.Store.ListAsync(ctx.Store.Query<AtcFinding>().Where(f => f.ExtractionId == id).Take(ChunkSize),
                    CancellationToken.None));
                if (findings.Count == 0)
                    break;
                await ctx.MutateAsync(() =>
                {
                    foreach (var f in findings)
                        ctx.Store.Remove(f);
                });
                await ctx.SaveAsync();
            }
            await ctx.MutateAsync(() => ctx.Store.Remove(e));
            await ctx.SaveAsync();
        }
    }
}
