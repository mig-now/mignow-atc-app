using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Features.Settings;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Scan;

namespace Mignow.Atc.Application.Features.Scans.Rules;

public sealed class ListScanRulesHandler(IDataStore store, IMignowRulesSource source) : IQueryHandler<ListScanRulesQuery, Result<ScanRulesResponse>>
{
    public async Task<Result<ScanRulesResponse>> Handle(ListScanRulesQuery query, CancellationToken ct)
    {
        var settings = await SettingsStore.LoadAsync(store, false, ct);
        var json = await source.ReadAsync(settings.MignowRulesPath, ct);
        return json.IsFailure ? json.Error : MignowRulesDocument.Parse(json.Value).ToResponse();
    }
}
