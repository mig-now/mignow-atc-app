using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.RemediationApply;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.SessionApply.ActivationDecision;

public sealed class ActivationDecisionHandler(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l)
    : ICommandHandler<ActivationDecisionCommand, Result<ActivationDecisionResponse>>
{
    public async Task<Result<ActivationDecisionResponse>> Handle(ActivationDecisionCommand c, CancellationToken ct)
    {
        var session = await SessionApplyLoader.SessionAsync(store, c.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var item = (await SessionApplyLoader.ItemsAsync(store, session.Id, [c.ItemId], true, ct)).FirstOrDefault();
        if (item is null)
            return l.NotFound("Item.NotFound");
        var app = item.Application;
        if (app is null || app.Result is not (ApplyResults.ActivationError or ApplyResults.RollbackActivationError))
            return l.Conflict("Apply.NoActivationError", item.N);
        if (c.Decision == "keepInactive")
        {
            item.SetApplication(app with { ErrorDecision = ApplyResults.DecisionKeepInactive });
            return new ActivationDecisionResponse(c.Decision, null);
        }
        if (app.BackupSource is null)
            return l.Conflict("Apply.NoBackup");
        var system = await SessionApplyLoader.SystemAsync(store, session.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(RunKind.Rollback, system, session.Guid, new RollbackParameters(session.Guid, item.Guid), ct);
        if (run.IsFailure)
            return run.Error;
        return new ActivationDecisionResponse(c.Decision, await RunViews.BuildAsync(store, run.Value, ct));
    }
}
