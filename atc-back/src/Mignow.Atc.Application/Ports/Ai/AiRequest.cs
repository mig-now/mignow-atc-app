using System.Text.Json;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Ports.Ai;

/// <summary>
/// Pedido de saída estruturada. <see cref="Purpose"/> identifica o prompt (ex.: <c>remediation.propose</c>): vai para o
/// log e escolhe a resposta da IA de desenvolvimento (Fake). <see cref="Schema"/> = JSON Schema da resposta.
/// </summary>
public sealed record AiRequest(AiUsage Usage, string Purpose, string System, string User, JsonElement Schema, AiEffort Effort = AiEffort.Medium,
    int? MaxOutputTokens = null);
