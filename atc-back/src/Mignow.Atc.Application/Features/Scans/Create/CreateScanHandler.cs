using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Scan;
using Mignow.Atc.Application.Features.Settings;
using Mignow.Atc.Application.Features.SapSystems;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Scan;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Features.Scans.Create;

public sealed class CreateScanHandler(IDataStore store, RunStarter starter, IMignowRulesSource rules, IStringLocalizer<Errors> l)
    : ICommandHandler<CreateScanCommand, Result<ScanStartedResponse>>
{
    public async Task<Result<ScanStartedResponse>> Handle(CreateScanCommand c, CancellationToken ct)
    {
        var system = await SapSystemLoader.LoadAsync(store, c.SapSystemId, false, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var extractions = store.Read<AtcExtraction>().Where(e => e.SapSystemId == system.Id);
        var extraction = c.ExtractionId is { } id
            ? await store.FirstOrDefaultAsync(extractions.Where(e => e.Guid == id), ct)
            : await store.FirstOrDefaultAsync(extractions.OrderByDescending(e => e.ExecutedAtUtc).ThenByDescending(e => e.Id), ct);
        if (extraction is null)
            return l.Conflict("Scan.NoExtraction");
        var settings = await SettingsStore.LoadAsync(store, false, ct);
        var json = await rules.ReadAsync(settings.MignowRulesPath, ct);
        if (json.IsFailure)
            return json.Error;
        var scan = MignowScan.Create(system.Id, extraction.Id, json.Value);
        scan.SetScope(Json.Write(new Dictionary<string, object> { ["incluir_gerados"] = c.IncludeGenerated }));
        store.Add(scan);
        var run = await starter.EnqueueAsync(RunKind.ScanInventory, system, scan.Guid, new ScanInventoryParameters(c.IncludeGenerated), ct);
        if (run.IsFailure)
            return run.Error;
        var view = await ScanViews.BuildAsync(store, scan, ct);
        return new ScanStartedResponse(view with { SapSystemName = system.DisplayName, SapSystemId = system.Guid }, (await RunViews.BuildAsync(store, [run.Value], ct))[0]);
    }
}
