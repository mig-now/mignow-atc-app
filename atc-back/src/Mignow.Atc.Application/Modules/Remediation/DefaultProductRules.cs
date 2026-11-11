namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Conteúdo do <c>produtos_mignow.json</c> (regras por produto MIGNOW): hoje só <c>MIGNOW_FSCM_PRODUCT</c>.</summary>
public sealed class DefaultProductRules : IProductRules
{
    private static readonly IReadOnlyList<ProductRule> Rules =
    [
        new("MIGNOW_FSCM_PRODUCT", "Objetos do produto MIGNOW de migração para FSCM Credit Management",
            new HashSet<string>(StringComparer.Ordinal) { "ZGFSCM_MIG_CREDITO", "ZGF_FSCM_MIG_CREDITO" }),
    ];

    public IReadOnlyList<ProductRule> Get() => Rules;
}
