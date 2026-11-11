namespace Mignow.Atc.Domain.Registry;

public enum AiProviderKind
{
    Anthropic,
    /// <summary>Determinístico, sem rede: só Development e testes.</summary>
    Fake,
}
