using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Visão mínima de um Run da sessão para o histórico (tempos, paralelismo e consumo de IA).</summary>
public sealed record HistoryRun(
    Guid Id, RunKind Kind, RunStatus Status, DateTimeOffset QueuedAt, DateTimeOffset? StartedAt, DateTimeOffset? FinishedAt, int Parallelism, int AiCalls,
    int AiInputTokens, int AiOutputTokens, decimal AiCost);
