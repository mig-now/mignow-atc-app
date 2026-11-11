using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;

namespace Mignow.Atc.Application.Features.TriageRules.List;

public sealed class ListTriageRulesHandler(IDataStore store, IProductRules products) : IQueryHandler<ListTriageRulesQuery, Result<IReadOnlyList<TriageRuleDefinitionResponse>>>
{
    public async Task<Result<IReadOnlyList<TriageRuleDefinitionResponse>>> Handle(ListTriageRulesQuery query, CancellationToken ct)
    {
        var all = await TriageRuleSource.LoadAsync(store, products, ct);
        var shown = query.ProjectType is { } type ? TriageEngine.RulesFor(type, all) : all;
        return Result.Success(TriageRuleDefinitionResponse.From(shown, all, products.Get()));
    }
}
