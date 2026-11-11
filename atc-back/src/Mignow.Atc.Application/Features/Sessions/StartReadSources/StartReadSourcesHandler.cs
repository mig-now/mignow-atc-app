using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.Remediation;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Sessions.StartReadSources;

public sealed class StartReadSourcesHandler(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l)
    : ICommandHandler<StartReadSourcesCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(StartReadSourcesCommand command, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, command.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        if (session.Archived)
            return l.Conflict("Session.Archived");
        var system = await SessionViews.LoadSystemAsync(store, session.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(RunKind.ReadSources, system, session.Guid,
            new ReadSourcesParameters(session.Guid, command.Batch ?? 0, command.IncludeStandard, command.Force), ct);
        return run.IsFailure ? run.Error : await RunViews.BuildAsync(store, run.Value, ct);
    }
}
