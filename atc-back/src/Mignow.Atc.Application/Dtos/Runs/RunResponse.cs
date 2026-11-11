using System.Text.Json;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Dtos.Runs;

/// <summary>Execução: tipo, ambiente, alvo, parâmetros e resumo (JSON livre do módulo), passos, consumo de IA.</summary>
public sealed record RunResponse(
    Guid Id, RunKind Kind, RunStatus Status, Guid CustomerId, string CustomerName, Guid SapSystemId, string SapSystemName, Guid? SubjectId,
    JsonElement? Parameters, JsonElement? Result, string StartedBy, string? CancelledBy, DateTimeOffset QueuedAt, DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt, DateTimeOffset? HeartbeatAt, string? FailureCode, IReadOnlyDictionary<string, string?>? FailureArgs, int Parallelism,
    int AiCalls, int AiInputTokens, int AiOutputTokens, decimal AiCost, int AiCallsWithoutCost, IReadOnlyList<RunStepResponse> Steps);
