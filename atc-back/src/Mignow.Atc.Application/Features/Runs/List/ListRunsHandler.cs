using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Runs.List;

public sealed class ListRunsHandler(IDataStore store) : IQueryHandler<ListRunsQuery, Result<PagedResult<RunResponse>>>
{
    public async Task<Result<PagedResult<RunResponse>>> Handle(ListRunsQuery query, CancellationToken ct)
    {
        var q = store.Including(store.Read<Run>(), nameof(Run.Steps));
        if (query.SapSystemId is not null)
        {
            var systemId = await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(s => s.Guid == query.SapSystemId).Select(s => (int?)s.Id), ct);
            q = q.Where(r => r.SapSystemId == (systemId ?? -1));
        }
        if (query.CustomerId is not null)
        {
            var customerId = await store.FirstOrDefaultAsync(store.Read<Customer>().Where(c => c.Guid == query.CustomerId).Select(c => (int?)c.Id), ct);
            q = q.Where(r => r.CustomerId == (customerId ?? -1));
        }
        if (query.Kind is not null)
            q = q.Where(r => r.Kind == query.Kind);
        if (query.SubjectId is not null)
            q = q.Where(r => r.SubjectId == query.SubjectId);
        if (query.ActiveOnly)
        {
            var active = Run.ActiveStatuses;
            q = q.Where(r => active.Contains(r.Status));
        }
        if (query.Statuses is { Count: > 0 })
        {
            var statuses = query.Statuses.Distinct().ToList();
            q = q.Where(r => statuses.Contains(r.Status));
        }
        var page = await store.PageAsync(q.OrderByDescending(r => r.QueuedAt).ThenByDescending(r => r.Id), query.Page, query.PageSize, ct);
        return new PagedResult<RunResponse>(await RunViews.BuildAsync(store, page.Items, ct), page.Page, page.PageSize, page.Total);
    }
}
