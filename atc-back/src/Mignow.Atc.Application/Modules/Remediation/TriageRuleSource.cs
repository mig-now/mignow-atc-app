using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Cadastro de triagem em uso: as regras padrão do código (<see cref="TriageEngine.Catalog"/>) com o que o consultor gravou por cima (código,
/// descrição, escopo, ativa, objetos da R7/R9, parâmetros da lógica e filtro adicional) e, no fim, as regras novas na ordem de criação.
/// </summary>
public static class TriageRuleSource
{
    public static async Task<IReadOnlyList<TriageRuleDefinition>> LoadAsync(IDataStore store, IProductRules products, CancellationToken ct) =>
        Merge(TriageEngine.Catalog(products.Get()), await store.ListAsync(store.Read<TriageRule>().OrderBy(r => r.Id), ct));

    public static IReadOnlyList<TriageRuleDefinition> Merge(IReadOnlyList<TriageRuleDefinition> defaults, IReadOnlyList<TriageRule> rows)
    {
        var saved = rows.Where(r => r.BuiltIn).ToDictionary(r => r.Key, StringComparer.OrdinalIgnoreCase);
        var builtIn = defaults.Select(d => saved.TryGetValue(d.RuleKey, out var r)
            ? d with
            {
                Key = d.RuleKey,
                Code = r.Code,
                Description = r.Description,
                Scope = r.Scope,
                Active = r.Active,
                RefObjects = d.UsesRefObjects ? r.RefObjects : d.RefObjects,
                // só os parâmetros que a regra tem, e só com valor (vazio = mantém o padrão)
                Parameters = d.Params.ToDictionary(p => p.Key,
                    p => r.Parameters.TryGetValue(p.Key, out var v) && v.Count > 0 ? v : p.Value, StringComparer.Ordinal),
                ObjectPattern = r.ObjectPattern,
                MessagePattern = r.MessagePattern,
            }
            : d with { Key = d.RuleKey });
        var keys = defaults.Select(d => d.RuleKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var custom = rows.Where(r => !r.BuiltIn && !keys.Contains(r.Key)).Select(ToDefinition);
        return [.. builtIn, .. custom];
    }

    public static TriageRuleDefinition ToDefinition(TriageRule r) =>
        new(r.Code, r.Description, r.Effect, r.Scope, false, r.Active, r.RefObjects, r.ObjectPattern, r.MessagePattern, r.Obs, Key: r.Key);
}
