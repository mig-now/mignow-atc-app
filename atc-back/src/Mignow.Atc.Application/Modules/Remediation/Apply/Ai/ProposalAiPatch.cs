using System.Text.Json.Serialization;

namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>
/// Trecho a corrigir: <c>antes</c> = linhas contíguas EXATAS do fonte, <c>depois</c> = o código novo sem marcadores, <c>linha</c> = linha do uso
/// (desempata trecho repetido), <c>tipo</c> = modificacao | delecao | insercao, <c>objeto_relacionado</c> = outro objeto alterado junto (ex.: interface).
/// </summary>
public sealed record ProposalAiPatch(
    [property: JsonPropertyName("antes")] IReadOnlyList<string> Before,
    [property: JsonPropertyName("depois")] IReadOnlyList<string> After,
    [property: JsonPropertyName("linha")] int? Line,
    [property: JsonPropertyName("tipo")] string Kind,
    [property: JsonPropertyName("objeto_relacionado")] string? RelatedObject);
