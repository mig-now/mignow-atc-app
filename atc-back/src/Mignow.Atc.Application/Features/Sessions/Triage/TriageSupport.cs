using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Triage;

internal static class TriageSupport
{
    public static async Task<(IReadOnlyList<RemediationItem> Items, TriageResult Result)> ClassifyAsync(
        IDataStore store, IProductRules products, RemediationSession session, bool tracked, CancellationToken ct)
    {
        var id = session.Id;
        var items = await store.ListAsync((tracked ? store.Query<RemediationItem>() : store.Read<RemediationItem>()).Where(i => i.SessionId == id).OrderBy(i => i.N), ct);
        var pending = items.Where(i => i.Status.Length == 0).Select(i => i.ObjectName).ToList();
        var sources = await SessionViews.SourcesAsync(store, session.SapSystemId, pending, ct);
        var rules = await TriageRuleSource.LoadAsync(store, products, ct);
        return (items, TriageEngine.Classify(items, sources, session.ProjectType, products.Get(), rules));
    }

    public static TriageResponse ToResponse(TriageResult result, int pending, IReadOnlyList<int>? applied) =>
        new(result.ProjectType, [.. result.Rules.Select(r => new TriageRuleResponse(r.Code, r.Description, r.Effect, r.Items.Count, r.Items, r.Key))], pending, applied);
}
