using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Extractions.List;

public sealed class ListExtractionsHandler(IDataStore store) : IQueryHandler<ListExtractionsQuery, Result<PagedResult<ExtractionResponse>>>
{
    public async Task<Result<PagedResult<ExtractionResponse>>> Handle(ListExtractionsQuery query, CancellationToken ct)
    {
        var q = store.Read<AtcExtraction>().Where(e => e.IsComplete);
        if (query.SapSystemId is not null)
        {
            var systemId = await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(s => s.Guid == query.SapSystemId).Select(s => (int?)s.Id), ct);
            q = q.Where(e => e.SapSystemId == (systemId ?? -1));
        }
        var page = await store.PageAsync(q.OrderByDescending(e => e.ExtractedAt).ThenByDescending(e => e.Id), query.Page, query.PageSize, ct);
        var ids = page.Items.Select(e => e.SapSystemId).Distinct().ToArray();
        var systems = (await store.ListAsync(store.Read<SapSystem>().Where(s => ids.Contains(s.Id)), ct)).ToDictionary(s => s.Id);
        return new PagedResult<ExtractionResponse>([.. page.Items.Select(e => e.ToResponse(systems[e.SapSystemId]))], page.Page, page.PageSize, page.Total);
    }
}
