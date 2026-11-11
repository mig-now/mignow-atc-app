using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Features.Scans.List;

public sealed class ListScansHandler(IDataStore store) : IQueryHandler<ListScansQuery, Result<PagedResult<ScanResponse>>>
{
    public async Task<Result<PagedResult<ScanResponse>>> Handle(ListScansQuery query, CancellationToken ct)
    {
        var q = store.Read<MignowScan>();
        if (query.SapSystemId is { } sid)
        {
            var ids = store.Read<SapSystem>().Where(s => s.Guid == sid).Select(s => s.Id);
            q = q.Where(s => ids.Contains(s.SapSystemId));
        }
        var page = await store.PageAsync(q.OrderByDescending(s => s.Id), query.Page, query.PageSize, ct);
        return new PagedResult<ScanResponse>(await ScanViews.BuildAsync(store, [.. page.Items], ct), page.Page, page.PageSize, page.Total);
    }
}
