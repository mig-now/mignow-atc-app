namespace Mignow.Atc.Infra.External.Ai.Providers;

/// <summary>Resposta do modelo: o JSON pedido, os tokens e o motivo da parada ("refusal"/"max_tokens" = tentativa falha).</summary>
public sealed record AiCompletion(string Json, long InputTokens, long OutputTokens, string? StopReason, decimal? Cost = null);
