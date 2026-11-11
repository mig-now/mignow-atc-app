using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Scans.SetScope;

public sealed class SetScopeHandler(IDataStore store, IStringLocalizer<Errors> l) : ICommandHandler<SetScopeCommand, Result<ScanResponse>>
{
    public async Task<Result<ScanResponse>> Handle(SetScopeCommand c, CancellationToken ct)
    {
        var scan = await ScanViews.LoadAsync(store, c.Id, true, ct);
        if (scan is null)
            return l.NotFound("Scan.NotFound");
        var known = MignowRulesDocument.Parse(scan.RulesSnapshotJson).Rules.Select(r => r.Id).ToHashSet(StringComparer.Ordinal);
        var unknown = c.RuleIds.Where(r => !known.Contains(r)).ToList();
        if (unknown.Count > 0)
            return l.Validation("Scan.UnknownRule", string.Join(", ", unknown));
        scan.SetRules(MignowRulesDocument.WithActive(scan.RulesSnapshotJson, [.. c.RuleIds]));
        return await ScanViews.BuildAsync(store, scan, ct);
    }
}
