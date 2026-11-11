using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// Histórico de processo por módulo (§3.8 do desenho): agrupa as execuções (Run) do mesmo alvo e calcula o tempo de
/// relógio, o tempo líquido e o consumo de IA. Tempo líquido = soma dos intervalos de trabalho, sem as pausas maiores que
/// <see cref="PauseSeconds"/> entre execuções (mesma regra do <c>tempo_liquido</c> do <c>resultado.py</c>, que ignora lacunas
/// acima de 10 min entre eventos). Pura: não acessa banco.
/// </summary>
public static class ProcessHistoryBuilder
{
    public const int PauseSeconds = 600;

    public static ProcessModule? ModuleOf(RunKind kind) => kind switch
    {
        RunKind.ExtractionCheck or RunKind.Extraction => ProcessModule.Extraction,
        RunKind.ReadSources or RunKind.ConfirmR1 or RunKind.Propose or RunKind.ApplySimulate or RunKind.Apply or RunKind.Rollback =>
            ProcessModule.Remediation,
        RunKind.ExemptionDiscover or RunKind.ExemptionSimulate or RunKind.ExemptionSend or RunKind.ExemptionCheck => ProcessModule.Exemption,
        RunKind.ScanInventory or RunKind.ScanSimulate or RunKind.ScanExecute or RunKind.ScanCleanup => ProcessModule.Scan,
        _ => null,
    };

    /// <summary>Nome da etapa nos relatórios (os mesmos do <c>coletar.py</c>).</summary>
    public static string StageName(ProcessModule module) => module switch
    {
        ProcessModule.Extraction => "Extração do ATC",
        ProcessModule.Remediation => "Remediação",
        ProcessModule.Exemption => "Pedido de isenção",
        _ => "Varredura MIGNOW",
    };

    public static IReadOnlyList<ProcessHistoryEntry> Build(IEnumerable<RunWithCalls> runs, ProcessModule? only = null)
    {
        var groups = runs
            .Select(r => (Run: r, Module: ModuleOf(r.Run.Kind)))
            .Where(x => x.Module is not null && (only is null || x.Module == only))
            .GroupBy(x => (Module: x.Module!.Value, x.Run.Run.SapSystemId, x.Run.Run.SubjectId));
        var entries = new List<ProcessHistoryEntry>();
        foreach (var g in groups)
        {
            var items = g.Select(x => x.Run).Where(r => Interval(r.Run) is not null).OrderBy(r => Interval(r.Run)!.Value.Start).ToList();
            if (items.Count == 0)
                continue;
            entries.Add(BuildEntry(g.Key.Module, g.Key.SapSystemId, g.Key.SubjectId, items));
        }
        return [.. entries.OrderBy(e => e.Start).ThenBy(e => e.Module)];
    }

    private static ProcessHistoryEntry BuildEntry(ProcessModule module, int systemId, Guid? subjectId, List<RunWithCalls> items)
    {
        var intervals = items.Select(r => Interval(r.Run)!.Value).OrderBy(i => i.Start).ToList();
        var start = intervals.Min(i => i.Start);
        var end = intervals.Max(i => i.End);
        var calls = items.SelectMany(r => r.AiCalls).ToList();
        var byModel = calls
            .GroupBy(c => (c.Usage, c.Model))
            .Select(g => new ProcessModelUsage(g.Key.Usage.ToString(), g.Key.Model, g.Count(), g.Count(c => !c.Ok), g.Sum(c => (long)c.InputTokens),
                g.Sum(c => (long)c.OutputTokens), g.Sum(c => c.Cost ?? 0m), g.Count(c => c.Cost is null)))
            .OrderBy(m => m.Usage, StringComparer.Ordinal).ThenBy(m => m.Model, StringComparer.Ordinal).ToList();
        var input = items.Sum(r => (long)r.Run.AiInputTokens);
        var output = items.Sum(r => (long)r.Run.AiOutputTokens);
        // O Run acumula os contadores de IA; as chamadas detalham por modelo. Sem Run contabilizado, vale a soma das chamadas.
        if (input == 0 && output == 0 && calls.Count > 0)
        {
            input = calls.Sum(c => (long)c.InputTokens);
            output = calls.Sum(c => (long)c.OutputTokens);
        }
        var parallel = items.Where(r => r.Run.Parallelism > 1)
            .Select(r => (Run: r.Run, I: Interval(r.Run)!.Value))
            .Select(x => new ProcessParallelPeriod(x.I.Start, x.I.End, x.Run.Parallelism, x.Run.Kind.ToString())).ToList();
        return new ProcessHistoryEntry(
            module, StageName(module), systemId, subjectId, start, end, (long)(end - start).TotalSeconds, NetSeconds(intervals), items.Count,
            items.Count(r => r.Run.Status == RunStatus.Completed), items.Count(r => r.Run.Status == RunStatus.Failed), input, output, input + output,
            items.Sum(r => r.Run.AiCost), items.Sum(r => r.Run.AiCallsWithoutCost), items.Sum(r => r.Run.AiCalls),
            items.Max(r => r.Run.Parallelism),
            items.GroupBy(r => r.Run.Kind.ToString()).OrderBy(g => g.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count()),
            byModel, parallel);
    }

    /// <summary>Intervalo de trabalho da execução: início (ou fila) até o fim (ou o último heartbeat). Sem início: ignorada.</summary>
    public static (DateTimeOffset Start, DateTimeOffset End)? Interval(Run run)
    {
        var start = run.StartedAt ?? (run.FinishedAt is null ? null : run.QueuedAt);
        if (start is null)
            return null;
        var end = run.FinishedAt ?? run.HeartbeatAt ?? start.Value;
        return (start.Value, end < start.Value ? start.Value : end);
    }

    /// <summary>União dos intervalos + lacunas curtas (≤ pausa); lacunas maiores que a pausa não contam.</summary>
    public static long NetSeconds(IReadOnlyList<(DateTimeOffset Start, DateTimeOffset End)> intervals, int pause = PauseSeconds)
    {
        var sorted = intervals.OrderBy(i => i.Start).ToList();
        double total = 0;
        var curStart = sorted[0].Start;
        var curEnd = sorted[0].End;
        foreach (var i in sorted.Skip(1))
        {
            if (i.Start <= curEnd)
            {
                if (i.End > curEnd)
                    curEnd = i.End;
                continue;
            }
            total += (curEnd - curStart).TotalSeconds;
            var gap = (i.Start - curEnd).TotalSeconds;
            if (gap <= pause)
                total += gap;
            curStart = i.Start;
            curEnd = i.End;
        }
        total += (curEnd - curStart).TotalSeconds;
        return (long)total;
    }
}
