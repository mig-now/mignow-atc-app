using System.Text.Json.Serialization;

namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>
/// Parecer da revisão (saída do especialista ABAP): <c>veredito</c> = ok | atencao | bloquear. Só <c>bloquear</c> impede a aplicação
/// automática (modo sem confirmação); o consultor decide nos demais casos.
/// </summary>
public sealed record ReviewAiResponse(
    [property: JsonPropertyName("veredito")] string Verdict,
    [property: JsonPropertyName("resumo")] string Summary,
    [property: JsonPropertyName("evidencias")] IReadOnlyList<string> Evidence,
    [property: JsonPropertyName("riscos")] IReadOnlyList<string> Risks,
    [property: JsonPropertyName("testes")] IReadOnlyList<string> Tests,
    [property: JsonPropertyName("validacao_humana")] IReadOnlyList<string> HumanValidation)
{
    public const string Ok = "ok";
    public const string Attention = "atencao";
    public const string Block = "bloquear";
}
