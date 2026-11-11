using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Report;

namespace Mignow.Atc.Application.Features.Reports.Highlights;

public sealed class GetHighlightsHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetHighlightsQuery, Result<HighlightsResponse>>
{
    public async Task<Result<HighlightsResponse>> Handle(GetHighlightsQuery query, CancellationToken ct)
    {
        var customer = await ReportLoader.FindCustomerAsync(store, query.CustomerId, ct);
        if (customer is null)
            return l.NotFound("Customer.NotFound");
        var h = await store.FirstOrDefaultAsync(store.Read<ReportHighlights>().Where(x => x.CustomerId == customer.Id), ct);
        return new HighlightsResponse(h?.Highlights ?? [], h?.Pending ?? [], h?.Premise);
    }
}
