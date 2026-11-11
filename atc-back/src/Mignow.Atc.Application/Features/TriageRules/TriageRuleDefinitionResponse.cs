using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.TriageRules;

/// <summary>
/// Regra do cadastro de triagem. <c>Effect</c> = status gravado ao aplicar, ou <c>signal</c> (só sinaliza); <c>Scope</c> = tipos de projeto em que vale.
/// Padrão (<c>BuiltIn</c>): lógica no código, edita descrição/escopo/ativa e, com <c>UsesRefObjects</c>, os objetos referenciados.
/// <c>Condition</c> = o que o motor verifica de fato; <c>Order</c> = posição na avaliação (o item fica com a primeira regra que casar).
/// <c>Key</c> = identidade fixa (R1..R9, produto; na nova, o código da criação) – a obs "Triagem R1: …" usa a chave; <c>Code</c> é renomeável.
/// <c>Parameters</c> = parâmetros da lógica da regra padrão (nome → valores; os nomes são os que a regra aceita).
/// </summary>
public sealed record TriageRuleDefinitionResponse(string Key, string Code, string Description, string Effect, TriageScope Scope, bool BuiltIn, bool Active,
    IReadOnlyList<string> RefObjects, string? ObjectPattern, string? MessagePattern, string? Obs, bool UsesRefObjects,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Parameters, string Condition, int Order)
{
    public static IReadOnlyList<TriageRuleDefinitionResponse> From(IReadOnlyList<TriageRuleDefinition> shown, IReadOnlyList<TriageRuleDefinition> all,
        IReadOnlyList<ProductRule> products)
    {
        var order = TriageRuleExplainer.Order(all, products);
        return [.. shown.Select(r => new TriageRuleDefinitionResponse(r.RuleKey, r.Code, r.Description, r.Effect, r.Scope, r.BuiltIn, r.Active, r.Refs,
            r.ObjectPattern, r.MessagePattern, r.Obs, r.UsesRefObjects, r.Params, TriageRuleExplainer.Condition(r, products),
            order.GetValueOrDefault(r.RuleKey)))];
    }
}
