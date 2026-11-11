using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Reports.GetData;

public sealed class GetReportHandler(IDataStore store, IStringLocalizer<Errors> l, IDateTimeProvider clock) : IQueryHandler<GetReportQuery, Result<ReportData>>
{
    public async Task<Result<ReportData>> Handle(GetReportQuery query, CancellationToken ct)
    {
        var loaded = await ReportLoader.LoadAsync(store, l, clock, query.CustomerId, query.SapSystemId, ct);
        return loaded.IsFailure ? loaded.Error : ReportDataBuilder.Build(loaded.Value.Input);
    }
}
