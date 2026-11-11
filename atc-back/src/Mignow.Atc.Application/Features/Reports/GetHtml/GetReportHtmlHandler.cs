using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Reports.GetHtml;

public sealed class GetReportHtmlHandler(IDataStore store, IStringLocalizer<Errors> l, IDateTimeProvider clock, IReportTemplate template)
    : IQueryHandler<GetReportHtmlQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetReportHtmlQuery query, CancellationToken ct)
    {
        var loaded = await ReportLoader.LoadAsync(store, l, clock, query.CustomerId, query.SapSystemId, ct);
        if (loaded.IsFailure)
            return loaded.Error;
        var data = ReportDataBuilder.Build(loaded.Value.Input);
        return ReportHtmlRenderer.Render(data, query.View ?? ReportHtmlRenderer.ViewFindings, template.Html).Document;
    }
}
