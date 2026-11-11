using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Revisão por IA (Review) das propostas simuladas, gravada em <c>Proposal.Review</c>. Sem gravação no SAP.</summary>
public sealed class ReviewStep(ReviewEngine engine) : IRunStep
{
    public string Code => RemediationApplyStepCodes.Review;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ApplyParameters>();
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, null, i => i.Status is RemediationStatus.ConfirmedKb or RemediationStatus.ConfirmedAi);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var n = await engine.ReviewAsync(data.Items, EngineHost.From(ctx));
        return StepOutcome.Info("Apply.Review.Done", Json.Args(("reviewed", n)));
    }
}
