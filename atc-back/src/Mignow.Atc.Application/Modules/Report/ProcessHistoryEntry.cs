namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// Uma linha do histórico de processo de um módulo: todas as execuções do mesmo alvo (sessão, lote, varredura ou extração).
/// Equivale a uma linha do <c>historico.json</c> das skills: tempo de relógio, tempo líquido (sem pausas acima de 10 min
/// entre execuções, como o <c>resultado.py</c>), tokens e custo por modelo, paralelismo.
/// </summary>
public sealed record ProcessHistoryEntry(
    ProcessModule Module,
    string Stage,
    int SapSystemId,
    Guid? SubjectId,
    DateTimeOffset Start,
    DateTimeOffset End,
    long ClockSeconds,
    long NetSeconds,
    int Runs,
    int RunsCompleted,
    int RunsFailed,
    long InputTokens,
    long OutputTokens,
    long TotalTokens,
    decimal Cost,
    int CallsWithoutCost,
    int AiCalls,
    int MaxParallelism,
    IReadOnlyDictionary<string, int> RunsByKind,
    IReadOnlyList<ProcessModelUsage> ByModel,
    IReadOnlyList<ProcessParallelPeriod> ParallelPeriods);
