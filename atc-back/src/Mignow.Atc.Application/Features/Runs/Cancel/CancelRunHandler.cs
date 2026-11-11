using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Identity;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Runs.Cancel;

public sealed class CancelRunHandler(IDataStore store, ICurrentUser user, IDateTimeProvider clock, IStringLocalizer<Errors> l)
    : ICommandHandler<CancelRunCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(CancelRunCommand command, CancellationToken ct)
    {
        var run = await RunLoader.LoadAsync(store, l, command.RunId, true, ct);
        if (run.IsFailure)
            return run.Error;
        if (!run.Value.IsActive)
            return l.Conflict("Run.NotActive");
        run.Value.RequestCancel(user.Actor, clock.UtcNow);
        store.Add(RunEvent.Create(run.Value.Id, clock.UtcNow, EventLevel.Warning, null, "Run.CancelRequested", Json.Args(("user", user.Actor))));
        return await RunViews.BuildAsync(store, run.Value, ct);
    }
}
