using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Scans.Get;

public sealed class GetScanHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetScanQuery, Result<ScanResponse>>
{
    public async Task<Result<ScanResponse>> Handle(GetScanQuery query, CancellationToken ct)
    {
        var scan = await ScanViews.LoadAsync(store, query.Id, false, ct);
        return scan is null ? l.NotFound("Scan.NotFound") : await ScanViews.BuildAsync(store, scan, ct);
    }
}
