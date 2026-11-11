using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.TriageRules.Save;

/// <summary>
/// Grava a regra: padrão vira (ou atualiza) a linha da chave fixa com o que o consultor mudou; nova é criada com os critérios. Código novo
/// (renomear) não pode repetir o código nem a chave de outra regra.
/// </summary>
public sealed class SaveTriageRuleHandler(IDataStore store, IProductRules products, IStringLocalizer<Errors> l)
    : ICommandHandler<SaveTriageRuleCommand, Result<TriageRuleDefinitionResponse>>
{
    public async Task<Result<TriageRuleDefinitionResponse>> Handle(SaveTriageRuleCommand c, CancellationToken ct)
    {
        var defaults = TriageEngine.Catalog(products.Get());
        var rows = await store.ListAsync(store.Query<TriageRule>().OrderBy(r => r.Id), ct);
        var current = TriageRuleSource.Merge(defaults, rows);
        var target = string.IsNullOrWhiteSpace(c.Code) ? null : c.Code.Trim().ToUpperInvariant();
        TriageRuleDefinition? rule = null;
        if (!c.IsNew)
        {
            rule = current.FirstOrDefault(r => r.Code.Equals(c.CurrentCode!.Trim(), StringComparison.OrdinalIgnoreCase));
            if (rule is null)
                return l.NotFound("TriageRule.NotFound");
        }
        if (target is not null && current.Any(r => r != rule && (r.Code.Equals(target, StringComparison.OrdinalIgnoreCase)
                || r.RuleKey.Equals(target, StringComparison.OrdinalIgnoreCase))))
            return l.Conflict("TriageRule.CodeExists", target);

        TriageRule row;
        if (rule is null)
        {
            if (!HasCriteria(c))
                return l.Validation("TriageRule.CriteriaRequired");
            row = TriageRule.Create(target!, c.Description, c.Effect, c.Scope, c.Active, c.RefObjects, c.ObjectPattern, c.MessagePattern, c.Obs);
            store.Add(row);
        }
        else if (rule.BuiltIn)
        {
            if (rule.UsesRefObjects && !(c.RefObjects ?? []).Any(r => !string.IsNullOrWhiteSpace(r)))
                return l.Validation("TriageRule.RefObjectsRequired", rule.Code);
            var parameters = c.Parameters ?? rule.Params;
            if (parameters.Keys.FirstOrDefault(k => !rule.Params.ContainsKey(k)) is { } unknown)
                return l.Validation("TriageRule.UnknownParameter", unknown, string.Join(", ", rule.Params.Keys));
            if (parameters.FirstOrDefault(p => !p.Value.Any(v => !string.IsNullOrWhiteSpace(v))) is { Key: { } empty })
                return l.Validation("TriageRule.ParameterRequired", empty);
            row = rows.FirstOrDefault(r => r.Key.Equals(rule.RuleKey, StringComparison.OrdinalIgnoreCase)) ?? AddBuiltIn(rule);
            row.UpdateBuiltIn(c.Description, c.Scope, c.Active, rule.UsesRefObjects ? c.RefObjects : null, parameters, c.ObjectPattern, c.MessagePattern);
        }
        else
        {
            if (!HasCriteria(c))
                return l.Validation("TriageRule.CriteriaRequired");
            row = rows.First(r => r.Key.Equals(rule.RuleKey, StringComparison.OrdinalIgnoreCase));
            row.UpdateCustom(c.Description, c.Effect, c.Scope, c.Active, c.RefObjects, c.ObjectPattern, c.MessagePattern, c.Obs);
        }
        if (target is not null)
            row.Rename(target);
        var all = TriageRuleSource.Merge(defaults, [.. rows.Where(r => r != row), row]);
        return TriageRuleDefinitionResponse.From([all.First(r => r.RuleKey.Equals(row.Key, StringComparison.OrdinalIgnoreCase))], all, products.Get())[0];
    }

    private TriageRule AddBuiltIn(TriageRuleDefinition rule)
    {
        var row = TriageRule.ForBuiltIn(rule.RuleKey, rule.Effect);
        store.Add(row);
        return row;
    }

    private static bool HasCriteria(SaveTriageRuleCommand c) =>
        (c.RefObjects ?? []).Any(r => !string.IsNullOrWhiteSpace(r)) || !string.IsNullOrWhiteSpace(c.ObjectPattern) || !string.IsNullOrWhiteSpace(c.MessagePattern);
}
