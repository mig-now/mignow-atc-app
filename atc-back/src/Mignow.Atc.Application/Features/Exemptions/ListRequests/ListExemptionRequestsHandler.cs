using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Exemptions.ListRequests;

public sealed class ListExemptionRequestsHandler(IDataStore store, IStringLocalizer<Errors> l)
    : IQueryHandler<ListExemptionRequestsQuery, Result<PagedResult<ExemptionRequestResponse>>>
{
    public async Task<Result<PagedResult<ExemptionRequestResponse>>> Handle(ListExemptionRequestsQuery query, CancellationToken ct)
    {
        var batch = await ExemptionBatchLoader.LoadAsync(store, l, query.BatchId, false, false, ct);
        if (batch.IsFailure)
            return batch.Error;
        var batchId = batch.Value.Id;
        var q = store.Read<ExemptionRequest>().Where(r => r.BatchId == batchId);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToUpperInvariant();
            q = status == ExemptionStatusMath.PendingKey ? q.Where(r => r.Status == null) : q.Where(r => r.Status == status);
        }
        var page = await store.PageAsync(q.OrderBy(r => r.Seq), query.Page, query.PageSize, ct);
        var itemIds = page.Items.Select(r => r.ItemId).Distinct().ToArray();
        var items = (await store.ListAsync(store.Read<RemediationItem>().Where(i => itemIds.Contains(i.Id)), ct)).ToDictionary(i => i.Id);
        return new PagedResult<ExemptionRequestResponse>([.. page.Items.Select(r => ExemptionViews.Request(r, items[r.ItemId]))], page.Page, page.PageSize, page.Total);
    }
}
