namespace Mignow.Atc.Application.Ports.Ai;

/// <summary>Consumo de uma chamada. <c>Cost</c> = valor informado pelo serviço (US$); nulo = não informado.</summary>
public sealed record AiUsageInfo(string Model, int InputTokens, int OutputTokens, decimal? Cost, bool Ok = true, string? ErrorCode = null);
