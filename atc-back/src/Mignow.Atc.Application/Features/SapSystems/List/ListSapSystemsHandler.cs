using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.List;

public sealed class ListSapSystemsHandler(IDataStore store) : IQueryHandler<ListSapSystemsQuery, Result<PagedResult<SapSystemResponse>>>
{
    public async Task<Result<PagedResult<SapSystemResponse>>> Handle(ListSapSystemsQuery query, CancellationToken ct)
    {
        var q = store.Including(store.Read<SapSystem>(), nameof(SapSystem.Customer));
        if (query.CustomerId is not null)
            q = q.Where(s => s.Customer.Guid == query.CustomerId);
        if (query.Active is not null)
            q = q.Where(s => s.Active == query.Active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(x => x.Sid.ToLower().Contains(s) || x.Host.ToLower().Contains(s) || x.Customer.Name.ToLower().Contains(s));
        }
        var page = await store.PageAsync(q.OrderBy(s => s.Customer.Name).ThenBy(s => s.Sid).ThenBy(s => s.Client), query.Page, query.PageSize, ct);
        return new PagedResult<SapSystemResponse>([.. page.Items.Select(s => s.ToResponse())], page.Page, page.PageSize, page.Total);
    }
}
