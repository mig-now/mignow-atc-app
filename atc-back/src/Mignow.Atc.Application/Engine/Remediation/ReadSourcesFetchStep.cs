using System.Text.Json;
using System.Text.RegularExpressions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>
/// 2º passo de <c>ReadSources</c> (porte de <c>ler_fonte.ps1</c>): para cada objeto do plano, fontes físicos pelo objectstructure (programa +
/// includes; classe = main + includes locais) e o texto de cada um, gravados no <see cref="SourceCache"/> (cache por objeto: quem já tem fonte lido
/// é pulado, salvo <c>Force</c>). Paralelo em <c>SourceReadParallelism</c>; erro de um objeto não para os demais; para no ponto seguro se o
/// consultor interromper. Idempotente.
/// </summary>
public sealed partial class ReadSourcesFetchStep(ISapReader reader) : IRunStep
{
    public string Code => RemediationStepCodes.ReadSourcesFetch;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ReadSourcesParameters>();
        var plan = ReadPlan(ctx.Run.ResultJson);
        if (plan is null)
            return StepOutcome.Stop("Remediation.ReadSourcesNoPlan");
        var parallelism = Math.Max(1, ctx.Settings.SourceReadParallelism);
        await ctx.SetParallelismAsync(Math.Min(parallelism, Math.Max(1, plan.Objects.Count)));
        int done = 0, read = 0, cached = 0, notFound = 0, errors = 0;
        var cancelled = false;
        await ctx.ReportProgressAsync(0, plan.Objects.Count);
        await Parallel.ForEachAsync(plan.Objects, new ParallelOptions { MaxDegreeOfParallelism = parallelism, CancellationToken = ctx.Ct }, async (o, _) =>
        {
            if (Volatile.Read(ref cancelled) || await ctx.IsCancellationRequestedAsync())
            {
                Volatile.Write(ref cancelled, true);
                return;
            }
            switch (await ReadObjectAsync(ctx, o, p.Force))
            {
                case ReadOutcome.Cached: Interlocked.Increment(ref cached); break;
                case ReadOutcome.Read: Interlocked.Increment(ref read); break;
                case ReadOutcome.NotFound: Interlocked.Increment(ref notFound); break;
                default: Interlocked.Increment(ref errors); break;
            }
            await ctx.ReportProgressAsync(Interlocked.Increment(ref done), plan.Objects.Count);
        });
        await ctx.MergeResultAsync(new
        {
            plan, objects = plan.Objects.Count, read, cached, notFound, errors, standardNotRead = plan.StandardNotRead.Count, cancelled,
        });
        if (cancelled)
            return StepOutcome.Cancel;
        var args = Json.Args(("objects", plan.Objects.Count), ("read", read), ("cached", cached), ("notFound", notFound), ("errors", errors));
        await ctx.LogAsync(EventLevel.Info, "Remediation.ReadSourcesDone", args);
        return StepOutcome.Info("Remediation.ReadSourcesDone", args);
    }

    private enum ReadOutcome
    {
        Cached,
        Read,
        NotFound,
        Error,
    }

    private async Task<ReadOutcome> ReadObjectAsync(RunContext ctx, ReadSourcesObject o, bool force)
    {
        var systemId = ctx.SapSystem.Id;
        var name = o.Name.ToUpperInvariant();
        if (!force && await ctx.WithDbAsync(() => ctx.Store.AnyAsync(ctx.Store.Read<SourceCache>().Where(c => c.SapSystemId == systemId && c.ObjectName == name), ctx.Ct)))
            return ReadOutcome.Cached;
        var parts = await reader.GetObjectSourcesAsync(ctx.SapConnection, o.Name, o.Type, ctx.Ct);
        if (parts.IsFailure)
        {
            await ctx.LogAsync(EventLevel.Warning, "Remediation.ReadSourceFailed", Json.Args(("type", o.Type), ("name", o.Name), ("message", parts.Error.Message)));
            return ReadOutcome.Error;
        }
        if (parts.Value.Count == 0)
        {
            await ctx.LogAsync(EventLevel.Warning, "Remediation.ObjectNotFound", Json.Args(("type", o.Type), ("name", o.Name)));
            return ReadOutcome.NotFound;
        }
        var texts = new List<(string Include, string Uri, string Text)>();
        foreach (var part in parts.Value)
        {
            var text = await reader.GetSourceAsync(ctx.SapConnection, part.Uri, true, ctx.Ct);
            if (text.IsFailure)
            {
                await ctx.LogAsync(EventLevel.Warning, "Remediation.ReadSourceFailed", Json.Args(("type", o.Type), ("name", part.Name), ("message", text.Error.Message)));
                continue;
            }
            texts.Add((FileName(part), part.Uri, text.Value));
        }
        if (texts.Count == 0)
            return ReadOutcome.Error;
        await ctx.WithDbAsync(async () =>
        {
            var existing = await ctx.Store.ListAsync(ctx.Store.Query<SourceCache>().Where(c => c.SapSystemId == systemId && c.ObjectName == name), ctx.Ct);
            foreach (var (include, uri, text) in texts)
            {
                var row = existing.FirstOrDefault(c => c.AdtUri == uri);
                if (row is null)
                    ctx.Store.Add(SourceCache.Create(systemId, name, include, uri, text, ctx.Clock.UtcNow));
                else
                    row.Refresh(text, ctx.Clock.UtcNow);
            }
        });
        await ctx.SaveAsync();
        return ReadOutcome.Read;
    }

    /// <summary>Nome do arquivo do fonte sem extensão: <c>&lt;nome&gt;[.parte]</c> com <c>\/:*?"&lt;&gt;|</c> trocados por <c>#</c>.</summary>
    public static string FileName(SapSourcePart part) =>
        InvalidChars().Replace(part.Name + (part.Part == "main" ? string.Empty : "." + part.Part), "#");

    private static ReadSourcesPlan? ReadPlan(string? resultJson)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
            return null;
        using var doc = JsonDocument.Parse(resultJson);
        return doc.RootElement.TryGetProperty("plan", out var plan) ? plan.Deserialize<ReadSourcesPlan>(Json.Options) : null;
    }

    [GeneratedRegex(@"[\\/:*?""<>|]")]
    private static partial Regex InvalidChars();
}
