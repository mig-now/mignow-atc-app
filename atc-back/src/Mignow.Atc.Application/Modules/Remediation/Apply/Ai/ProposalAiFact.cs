using System.Text.Json.Serialization;

namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>Fato técnico verificado (sem cliente, request, objeto do cliente ou linha), candidato a entrar na KB.</summary>
public sealed record ProposalAiFact(
    [property: JsonPropertyName("elemento")] string Element,
    [property: JsonPropertyName("fato")] string Fact,
    [property: JsonPropertyName("prova")] string? Proof,
    [property: JsonPropertyName("impacto")] string? Impact);
