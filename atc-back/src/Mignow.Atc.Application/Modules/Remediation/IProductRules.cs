namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Fonte das regras por produto MIGNOW (o padrão é o conteúdo do <c>produtos_mignow.json</c> da skill).</summary>
public interface IProductRules
{
    IReadOnlyList<ProductRule> Get();
}
