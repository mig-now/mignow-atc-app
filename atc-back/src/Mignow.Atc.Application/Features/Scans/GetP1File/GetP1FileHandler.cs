using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Scans.GetP1File;

public sealed class GetP1FileHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetP1FileQuery, Result<ScanFileResponse>>
{
    public async Task<Result<ScanFileResponse>> Handle(GetP1FileQuery query, CancellationToken ct)
    {
        var scan = await ScanViews.LoadAsync(store, query.Id, false, ct);
        if (scan is null)
            return l.NotFound("Scan.NotFound");
        var p1 = await ScanP1Source.LoadAsync(store, l, scan, query.IncludeOtherLine, ct);
        return p1.IsFailure ? p1.Error : new ScanFileResponse(p1.Value.FileName, "text/csv; charset=utf-8", p1.Value.Csv);
    }
}
