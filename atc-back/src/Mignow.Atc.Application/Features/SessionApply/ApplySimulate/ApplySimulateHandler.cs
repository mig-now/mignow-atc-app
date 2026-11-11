using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.RemediationApply;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.SessionApply.ApplySimulate;

public sealed class ApplySimulateHandler(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l) : ICommandHandler<ApplySimulateCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(ApplySimulateCommand c, CancellationToken ct)
    {
        var session = await SessionApplyLoader.SessionAsync(store, c.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var items = await SessionApplyLoader.ItemsAsync(store, session.Id, c.ItemIds, false, ct);
        if (items.Count != c.ItemIds.Distinct().Count())
            return l.NotFound("Item.NotFound");
        if (items.FirstOrDefault(i => i.Status is not (RemediationStatus.ConfirmedKb or RemediationStatus.ConfirmedAi) || i.Preparation?.NewSource is null) is { } bad)
            return l.Conflict("Apply.ItemNotReady", bad.N);
        var system = await SessionApplyLoader.SystemAsync(store, session.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(RunKind.ApplySimulate, system, session.Guid, new ApplyParameters(session.Guid, c.ItemIds, c.AllowTableStructure, c.DdicCreate), ct);
        if (run.IsFailure)
            return run.Error;
        return await RunViews.BuildAsync(store, run.Value, ct);
    }
}
