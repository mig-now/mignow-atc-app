using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Dtos.Runs;

/// <summary>Uma chamada de IA da execução: uso, modelo, tokens, custo (US$, nulo = não informado) e se deu certo.</summary>
public sealed record RunAiCallResponse(long Id, AiUsage Usage, string Model, int InputTokens, int OutputTokens, decimal? Cost, bool Ok,
    string? ErrorCode, string? ItemRef, DateTimeOffset At);
