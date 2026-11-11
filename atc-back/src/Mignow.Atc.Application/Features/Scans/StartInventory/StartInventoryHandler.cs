using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Scan;
using Mignow.Atc.Application.Features.SapSystems;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Scans.StartInventory;

public sealed class StartInventoryHandler(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l) : ICommandHandler<StartInventoryCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(StartInventoryCommand command, CancellationToken ct)
    {
        var scan = await ScanViews.LoadAsync(store, command.Id, false, ct);
        if (scan is null)
            return l.NotFound("Scan.NotFound");
        var system = await store.FirstOrDefaultAsync(store.Including(store.Read<Mignow.Atc.Domain.Registry.SapSystem>(), nameof(Mignow.Atc.Domain.Registry.SapSystem.Customer)).Where(s => s.Id == scan.SapSystemId), ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(RunKind.ScanInventory, system, scan.Guid, new ScanInventoryParameters(command.IncludeGenerated), ct);
        if (run.IsFailure)
            return run.Error;
        return (await RunViews.BuildAsync(store, [run.Value], ct))[0];
    }
}
