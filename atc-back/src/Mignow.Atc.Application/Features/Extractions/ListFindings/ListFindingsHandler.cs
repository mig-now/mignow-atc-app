using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Extraction;

namespace Mignow.Atc.Application.Features.Extractions.ListFindings;

public sealed class ListFindingsHandler(IDataStore store, IStringLocalizer<Errors> l)
    : IQueryHandler<ListFindingsQuery, Result<PagedResult<FindingResponse>>>
{
    public async Task<Result<PagedResult<FindingResponse>>> Handle(ListFindingsQuery query, CancellationToken ct)
    {
        var extractionId = await store.FirstOrDefaultAsync(
            store.Read<AtcExtraction>().Where(e => e.Guid == query.ExtractionId && e.IsComplete).Select(e => (int?)e.Id), ct);
        if (extractionId is null)
            return l.NotFound("Extraction.NotFound");
        var id = extractionId.Value;
        var q = store.Read<AtcFinding>().Where(f => f.ExtractionId == id);
        if (query.Priority is { } priority)
            q = q.Where(f => f.Priority == priority);
        if (!string.IsNullOrWhiteSpace(query.Object))
        {
            var name = query.Object.Trim().ToUpperInvariant();
            q = q.Where(f => f.ObjectName.ToUpper().Contains(name));
        }
        if (query.InBaseline is { } inBaseline)
            q = q.Where(f => f.InBaseline == inBaseline);
        var page = await store.PageAsync(q.OrderBy(f => f.Seq), query.Page, query.PageSize, ct);
        return new PagedResult<FindingResponse>([.. page.Items.Select(f => f.ToResponse(query.IncludeColumns))], page.Page, page.PageSize, page.Total);
    }
}
