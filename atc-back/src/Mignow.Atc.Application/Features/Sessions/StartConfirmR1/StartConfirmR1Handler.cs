using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.Remediation;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Sessions.StartConfirmR1;

public sealed class StartConfirmR1Handler(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l)
    : ICommandHandler<StartConfirmR1Command, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(StartConfirmR1Command command, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, command.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        if (session.Archived)
            return l.Conflict("Session.Archived");
        var system = await SessionViews.LoadSystemAsync(store, session.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(RunKind.ConfirmR1, system, session.Guid, new ConfirmR1Parameters(session.Guid, command.ItemNs, command.Apply), ct);
        return run.IsFailure ? run.Error : await RunViews.BuildAsync(store, run.Value, ct);
    }
}
