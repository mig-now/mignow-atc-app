using System.Text.Json;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>
/// Cruzamento com o CSV total da extração (porte de <c>cruzar.py</c>): classifica as ocorrências pendentes, grava o resumo
/// e deixa o CSV P1 pronto (gerado sob demanda). Idempotente. Só roda com todos os lotes OK; senão avisa e segue.
/// </summary>
public sealed class ScanCrossStep : IRunStep
{
    private const int FindingsPage = 2000;

    public string Code => ScanStepCodes.Cross;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var scan = await ScanRunSupport.LoadScanAsync(ctx);
        if (scan is null)
            return StepOutcome.Stop("Scan.NotFound");
        var missing = scan.Batches.Where(b => !b.IsOk).Select(b => b.Number).ToList();
        if (scan.Batches.Count == 0 || missing.Count > 0)
            return StepOutcome.Info("Scan.CrossSkippedLotsMissing", Json.Args(("lots", string.Join(", ", missing))));
        if (scan.ExtractionId is null)
            return StepOutcome.Stop("Scan.NoExtraction");
        var p = ctx.Parameters<ScanExecuteParameters>();
        await ctx.ReportProgressAsync(0, 1);

        var extractionId = scan.ExtractionId.Value;
        var extraction = await ctx.WithDbAsync(() => ctx.Store.FirstOrDefaultAsync(ctx.Store.Read<AtcExtraction>().Where(e => e.Id == extractionId), ctx.Ct));
        if (extraction is null)
            return StepOutcome.Stop("Scan.NoExtraction");
        var findings = new List<ScanAtcFinding>();
        for (var page = 0; ; page++)
        {
            var skip = page * FindingsPage;
            var rows = await ctx.WithDbAsync(() => ctx.Store.ListAsync(
                ctx.Store.Read<AtcFinding>().Where(f => f.ExtractionId == extractionId).OrderBy(f => f.Seq).Skip(skip).Take(FindingsPage)
                    .Select(f => new FindingRow(f.AllColumnsJson, f.RefObjectName)), ctx.Ct));
            findings.AddRange(rows.Select(Reduce));
            if (rows.Count < FindingsPage)
                break;
        }

        var scanId = scan.Id;
        var occurrences = await ctx.WithDbAsync(() => ctx.Store.ListAsync(
            ctx.Store.Query<MignowOccurrence>().Where(o => o.ScanId == scanId).OrderBy(o => o.Batch).ThenBy(o => o.Id), ctx.Ct));
        var rules = MignowRulesDocument.Parse(scan.RulesSnapshotJson);
        var now = ctx.Clock.UtcNow.ToLocalTime().DateTime;
        var sid = ctx.SapSystem.Sid;
        var client = ctx.SapSystem.Client;
        var atcBase = new ScanAtcBase(ExtractionNaming.FileName(
            ExtractionNaming.BaseName(sid, client, extraction.Series, extraction.ExecutedAtUtc, extraction.DisplayId), ExtractionFileKind.Total),
            extraction.Series, extraction.ExecutedAtUtc?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
        ScanCrossResult result = null!;
        await ctx.MutateAsync(() =>
        {
            result = ScanCrossAnalyzer.Run(new ScanCrossInput(occurrences, scan.Batches, ScanRunSupport.Counts(scan.ScopeCountsJson), rules, findings, atcBase,
                $"varredura_{scan.CreatedAt.ToLocalTime():yyyyMMdd-HHmmss}", sid, client, ScanCodeGenerator.DefaultClass, p.IncludeOtherLine, now));
            scan.SetSummary(result.Summary.ToJsonString());
        });
        await ctx.SaveAsync();
        var pending = result.Summary["cruzamento_pendentes"];
        await ctx.SetResultAsync(new
        {
            occurrences = occurrences.Count,
            crossPending = pending,
            mignow = result.Summary["mignow"],
            p1File = result.P1FileName,
            p1Rows = result.P1Rows,
        });
        await ctx.ReportProgressAsync(1, 1);
        await ctx.LogAsync(EventLevel.Info, "Scan.CrossDone", Json.Args(("occurrences", occurrences.Count), ("p1", result.P1Rows)));
        return StepOutcome.Done;
    }

    private sealed record FindingRow(string AllColumnsJson, string? RefObjectName);

    /// <summary>location e tag_REF_OBJ_NAME do finding (colunas do CSV total em <c>AllColumnsJson</c>; sem a coluna, usa o campo indexado).</summary>
    private static ScanAtcFinding Reduce(FindingRow row)
    {
        var columns = ExtractionFiles.ParseRow(row.AllColumnsJson);
        var location = columns.GetValueOrDefault("location") ?? string.Empty;
        var reference = columns.TryGetValue("tag_REF_OBJ_NAME", out var r) ? r : row.RefObjectName ?? string.Empty;
        return new ScanAtcFinding(location, reference);
    }
}
