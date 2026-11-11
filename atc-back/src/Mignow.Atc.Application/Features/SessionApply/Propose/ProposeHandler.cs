using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.RemediationApply;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.SessionApply.Propose;

public sealed class ProposeHandler(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l) : ICommandHandler<ProposeCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(ProposeCommand c, CancellationToken ct)
    {
        var session = await SessionApplyLoader.SessionAsync(store, c.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        if (c.AutoConfirm && !session.AutoMode)
            return l.Conflict("Apply.AutoModeRequired");
        var system = await SessionApplyLoader.SystemAsync(store, session.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(RunKind.Propose, system, session.Guid, new ProposeParameters(session.Guid, c.ItemIds, c.GroupKeys, c.Mode, c.AutoConfirm), ct);
        if (run.IsFailure)
            return run.Error;
        return await RunViews.BuildAsync(store, run.Value, ct);
    }
}
