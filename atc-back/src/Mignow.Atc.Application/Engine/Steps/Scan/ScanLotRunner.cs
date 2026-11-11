using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>
/// Executa processos (uma classe temporária cada) em paralelo: gera o ABAP de cada lote, chama a porta, interpreta a saída
/// e grava, a cada lote, o status, as estatísticas e as ocorrências (retomável). O consultor interrompe entre lotes.
/// </summary>
public sealed class ScanLotRunner(IMignowScanner scanner)
{
    public async Task<ScanProcessOutcome> RunAsync(
        RunContext ctx, MignowScan scan, IReadOnlyList<(string ClassName, IReadOnlyList<int> Lots)> processes, ScanExecuteParameters p, string rulesJson)
    {
        var rules = MignowRulesDocument.Parse(rulesJson);
        var specs = rules.ActiveRules.Select(r => new ScanRuleSpec(r.Id, r.Pcre, r.Text, r.FixedElement, r.Requires, r.SuppressedBy, r.OriginalPcre)).ToList();
        var batches = scan.Batches.ToDictionary(b => b.Number);
        var generatedAt = ctx.Clock.UtcNow.ToLocalTime().DateTime;
        var failed = new List<string>();
        var errors = new List<string>();
        var notRemoved = new List<string>();
        var done = new List<int>();
        var cancelled = false;
        var gate = new object();

        async Task RunProcess((string ClassName, IReadOnlyList<int> Lots) pr)
        {
            var requests = new List<ScanLotRequest>();
            foreach (var n in pr.Lots)
            {
                var b = batches[n];
                var objects = b.Objects.Select(o => (o.Type, o.Name)).ToList();
                string source;
                try
                {
                    source = ScanCodeGenerator.Generate(pr.ClassName, n, objects, rules, p.CompareOriginal, generatedAt);
                }
                catch (ScanGenerationException ex)
                {
                    lock (gate) errors.Add(ex.Message);
                    return;
                }
                requests.Add(new ScanLotRequest(n, source, [.. b.Objects.Select(o => new ScanLotObject(o.Type, o.Name))], rules.Trigger, specs, p.CompareOriginal));
            }
            var touched = new List<int>();
            var started = ctx.Clock.UtcNow;

            async Task<bool> OnLot(ScanLotResult res)
            {
                var parsed = ScanOutputParser.Parse(res.Output);
                var status = res.Status;
                var message = res.Message;
                if (status == ScanBatchStatus.Ok && !parsed.Complete)
                {
                    status = ScanBatchStatus.ExecutionError;
                    message = "saída sem a linha final Z (execução interrompida?)";
                }
                var stats = parsed.ToStats(res.ElapsedSeconds, message, res.Messages, started);
                lock (gate) touched.Add(res.Number);
                await ctx.WithDbAsync(async () =>
                {
                    if (status == ScanBatchStatus.Ok)
                    {
                        var number = res.Number;
                        var scanId = scan.Id;
                        foreach (var old in await ctx.Store.ListAsync(ctx.Store.Query<MignowOccurrence>().Where(o => o.ScanId == scanId && o.Batch == number), CancellationToken.None))
                            ctx.Store.Remove(old);
                        var seen = new HashSet<(string, int, string, string)>();
                        foreach (var h in parsed.Hits)
                        {
                            if (!seen.Add((h.Source, h.Line, h.RuleId, h.Element)))
                                continue;
                            var o = MignowOccurrence.Create(scanId, h.RuleId, h.Name, h.Type, IncludeOf(h.Source), h.Source, h.Line, h.LineEnd, h.Snippet, h.Element, number, h.Flag);
                            ctx.Store.Add(o);
                        }
                    }
                    var cur = scan.Batches.First(b => b.Number == res.Number);
                    scan.ReplaceBatch(cur with { Status = status, ClassName = pr.ClassName, ClassRemoved = false, Stats = stats });
                });
                await ctx.SaveAsync();
                if (status == ScanBatchStatus.Ok)
                    lock (gate) done.Add(res.Number);
                else
                    lock (gate) failed.Add($"{res.Number}:{status}");
                await ctx.LogAsync(status == ScanBatchStatus.Ok ? EventLevel.Info : EventLevel.Error, status == ScanBatchStatus.Ok ? "Scan.LotDone" : "Scan.LotFailed",
                    Json.Args(("lot", res.Number), ("status", status), ("seconds", res.ElapsedSeconds), ("occurrences", stats.Occurrences), ("message", message)), $"lot:{res.Number}");
                if (await ctx.IsCancellationRequestedAsync())
                {
                    lock (gate) cancelled = true;
                    return false;
                }
                return status == ScanBatchStatus.Ok;
            }

            var result = await scanner.ExecuteProcessAsync(ctx.SapConnection, new ScanProcessRequest(pr.ClassName, requests), OnLot, ctx.Ct);
            if (result.IsFailure)
            {
                lock (gate) errors.Add($"{pr.ClassName}: {result.Error.Code}");
                return;
            }
            var r = result.Value;
            if (r.Error is not null)
                lock (gate) errors.Add($"{pr.ClassName}: {r.Error}");
            if (!r.Removal.Removed)
                lock (gate) notRemoved.Add(pr.ClassName);
            if (touched.Count > 0)
            {
                await ctx.MutateAsync(() =>
                {
                    foreach (var n in touched)
                        scan.ReplaceBatch(scan.Batches.First(b => b.Number == n) with { ClassRemoved = r.Removal.Removed });
                });
                await ctx.SaveAsync();
            }
        }

        await ctx.SetParallelismAsync(processes.Count);
        await Task.WhenAll(processes.Select(RunProcess));
        return new ScanProcessOutcome(failed, errors, notRemoved, cancelled, [.. done.Order()]);
    }

    /// <summary>Include/módulo/classe-include do caminho do fonte (<c>…/includes/x</c>, <c>…/fmodules/x</c>, <c>#include=x</c>); nulo se é o fonte principal.</summary>
    internal static string? IncludeOf(string source)
    {
        var hash = source.IndexOf("#include=", StringComparison.Ordinal);
        if (hash >= 0)
            return source[(hash + "#include=".Length)..];
        foreach (var marker in new[] { "/includes/", "/fmodules/" })
        {
            var i = source.IndexOf(marker, StringComparison.Ordinal);
            if (i >= 0)
                return source[(i + marker.Length)..];
        }
        return null;
    }
}
