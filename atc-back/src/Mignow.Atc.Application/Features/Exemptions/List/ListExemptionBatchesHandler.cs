using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Exemptions.List;

public sealed class ListExemptionBatchesHandler(IDataStore store) : IQueryHandler<ListExemptionBatchesQuery, Result<PagedResult<ExemptionBatchResponse>>>
{
    public async Task<Result<PagedResult<ExemptionBatchResponse>>> Handle(ListExemptionBatchesQuery query, CancellationToken ct)
    {
        var q = store.Read<ExemptionBatch>();
        if (query.SapSystemId is { } systemGuid)
            q = q.Where(b => store.Read<SapSystem>().Any(s => s.Guid == systemGuid && s.Id == b.SapSystemId));
        if (query.SessionId is { } sessionGuid)
            q = q.Where(b => store.Read<RemediationSession>().Any(s => s.Guid == sessionGuid && s.Id == b.SessionId));
        var page = await store.PageAsync(q.OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.Id), query.Page, query.PageSize, ct);
        return new PagedResult<ExemptionBatchResponse>(await ExemptionViews.BatchesAsync(store, page.Items, ct), page.Page, page.PageSize, page.Total);
    }
}
