using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Features.SapSystems;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Extractions.Check;

public sealed class StartExtractionCheckHandler(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l)
    : ICommandHandler<StartExtractionCheckCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(StartExtractionCheckCommand command, CancellationToken ct)
    {
        var system = await SapSystemLoader.LoadAsync(store, command.SapSystemId, false, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(RunKind.ExtractionCheck, system, system.Guid, null, ct);
        if (run.IsFailure)
            return run.Error;
        return (await RunViews.BuildAsync(store, [run.Value], ct))[0];
    }
}
