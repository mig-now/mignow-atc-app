using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Infra.External.Ai.Providers;

/// <summary>
/// Teto de saída e tempo das chamadas. O teto conta raciocínio + resposta e o serviço cobra só o que o modelo gera: teto
/// folgado não custa nada; teto curto transforma resposta longa em falha paga. Limitado ao máximo do modelo (Models API).
/// </summary>
public static class AiLimits
{
    /// <summary>Tempo de cada tentativa HTTP ao Claude (streaming; o SDK repete 2× em falha).</summary>
    public static readonly TimeSpan AnthropicTimeout = TimeSpan.FromMinutes(20);

    /// <summary>Teto quando o máximo do modelo não pôde ser consultado (aceito por todos os Claude atuais).</summary>
    public const int FallbackModelMax = 32_000;

    /// <summary>Teto padrão por uso (o pedido pode informar outro em <c>MaxOutputTokens</c>).</summary>
    public static int Default(AiUsage usage) => usage switch
    {
        AiUsage.Analysis => 64_000,
        AiUsage.Review => 32_000,
        _ => 16_000,
    };
}
