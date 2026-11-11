using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Report;

namespace Mignow.Atc.Application.Features.Reports.Highlights;

public sealed class UpdateHighlightsHandler(IDataStore store, IStringLocalizer<Errors> l) : ICommandHandler<UpdateHighlightsCommand, Result<HighlightsResponse>>
{
    public async Task<Result<HighlightsResponse>> Handle(UpdateHighlightsCommand command, CancellationToken ct)
    {
        var customer = await ReportLoader.FindCustomerAsync(store, command.CustomerId, ct);
        if (customer is null)
            return l.NotFound("Customer.NotFound");
        var h = await store.FirstOrDefaultAsync(store.Query<ReportHighlights>().Where(x => x.CustomerId == customer.Id), ct);
        if (h is null)
        {
            h = ReportHighlights.Create(customer.Id);
            store.Add(h);
        }
        h.Update(command.Highlights, command.Pending, command.Premise);
        return new HighlightsResponse(h.Highlights, h.Pending, h.Premise);
    }
}
