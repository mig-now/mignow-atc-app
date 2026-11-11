using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Features.Scans.ListOccurrences;

public sealed class ListOccurrencesHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<ListOccurrencesQuery, Result<PagedResult<ScanOccurrenceResponse>>>
{
    public async Task<Result<PagedResult<ScanOccurrenceResponse>>> Handle(ListOccurrencesQuery query, CancellationToken ct)
    {
        var scan = await ScanViews.LoadAsync(store, query.Id, false, ct);
        if (scan is null)
            return l.NotFound("Scan.NotFound");
        var scanId = scan.Id;
        var q = store.Read<MignowOccurrence>().Where(o => o.ScanId == scanId);
        if (!string.IsNullOrEmpty(query.RuleId))
            q = q.Where(o => o.RuleId == query.RuleId);
        if (!string.IsNullOrEmpty(query.Cross))
            q = q.Where(o => o.Cross == query.Cross);
        if (!string.IsNullOrEmpty(query.Status))
            q = q.Where(o => o.Class == query.Status);
        if (!string.IsNullOrWhiteSpace(query.ObjectName))
        {
            var name = query.ObjectName.Trim().ToUpper();
            q = q.Where(o => o.ObjectName.ToUpper().Contains(name));
        }
        var page = await store.PageAsync(q.OrderBy(o => o.Batch).ThenBy(o => o.Id), query.Page, query.PageSize, ct);
        return new PagedResult<ScanOccurrenceResponse>([.. page.Items.Select(o => o.ToResponse())], page.Page, page.PageSize, page.Total);
    }
}
