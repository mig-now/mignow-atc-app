using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.List;

public sealed class ListSessionsHandler(IDataStore store) : IQueryHandler<ListSessionsQuery, Result<PagedResult<SessionResponse>>>
{
    public async Task<Result<PagedResult<SessionResponse>>> Handle(ListSessionsQuery query, CancellationToken ct)
    {
        var q = store.Read<RemediationSession>();
        if (query.SapSystemId is { } sid)
            q = q.Where(s => store.Read<SapSystem>().Any(x => x.Guid == sid && x.Id == s.SapSystemId));
        if (query.CustomerId is { } cid)
            q = q.Where(s => store.Read<Customer>().Any(x => x.Guid == cid && x.Id == s.CustomerId));
        if (query.Archived is { } archived)
            q = q.Where(s => s.Archived == archived);
        var page = await store.PageAsync(q.OrderByDescending(s => s.CreatedAt), query.Page, query.PageSize, ct);
        return new PagedResult<SessionResponse>(await SessionViews.BuildAsync(store, [.. page.Items], ct), page.Page, page.PageSize, page.Total);
    }
}
