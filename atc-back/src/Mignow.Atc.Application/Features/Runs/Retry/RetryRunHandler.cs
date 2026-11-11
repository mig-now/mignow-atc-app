using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Execution;
using Mignow.Atc.Application.Ports.Identity;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Runs.Retry;

public sealed class RetryRunHandler(IDataStore store, ICurrentUser user, IDateTimeProvider clock, IStringLocalizer<Errors> l)
    : ICommandHandler<RetryRunCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(RetryRunCommand command, CancellationToken ct)
    {
        var run = await RunLoader.LoadAsync(store, l, command.RunId, true, ct);
        if (run.IsFailure)
            return run.Error;
        if (run.Value.Status != RunStatus.Failed)
            return l.Conflict("Run.NotFailed");
        // Volta a ser ativa: respeita uma execução por ambiente (o índice único também barra no commit).
        var free = await RunLock.EnsureFreeAsync(store, run.Value.SapSystemId, l, ct);
        if (free.IsFailure)
            return free.Error;
        var failure = run.Value.FailureCode;
        run.Value.Retry();
        store.Add(RunEvent.Create(run.Value.Id, clock.UtcNow, EventLevel.Info, null, "Run.RetryRequested",
            Json.Args(("user", user.Actor), ("failure", failure))));
        return await RunViews.BuildAsync(store, run.Value, ct);
    }
}
