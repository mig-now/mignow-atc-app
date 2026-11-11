using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.TriageRules.Delete;

public sealed class DeleteTriageRuleHandler(IDataStore store, IProductRules products, IStringLocalizer<Errors> l) : ICommandHandler<DeleteTriageRuleCommand, Result>
{
    public async Task<Result> Handle(DeleteTriageRuleCommand command, CancellationToken ct)
    {
        var rows = await store.ListAsync(store.Query<TriageRule>().OrderBy(r => r.Id), ct);
        var rule = TriageRuleSource.Merge(TriageEngine.Catalog(products.Get()), rows)
            .FirstOrDefault(r => r.Code.Equals(command.Code.Trim(), StringComparison.OrdinalIgnoreCase));
        if (rule is null)
            return Result.Failure(l.NotFound("TriageRule.NotFound"));
        if (rule.BuiltIn)
            return Result.Failure(l.Conflict("TriageRule.BuiltInCannotDelete", rule.Code));
        store.Remove(rows.First(r => r.Key.Equals(rule.RuleKey, StringComparison.OrdinalIgnoreCase)));
        return Result.Success();
    }
}
