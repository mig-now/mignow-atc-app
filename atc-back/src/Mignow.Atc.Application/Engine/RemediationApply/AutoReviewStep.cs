using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Modo sem confirmação: revisão por IA do simulado. Veredito <c>bloquear</c> impede a aplicação automática do item (recomendação da IA).</summary>
public sealed class AutoReviewStep(ReviewEngine engine) : IRunStep
{
    public string Code => RemediationApplyStepCodes.AutoReview;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ProposeParameters>();
        if (!p.AutoConfirm)
            return StepOutcome.Done;
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, p.GroupKeys, i => AutoSelection.Simulated(i, p.Reprocess));
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var n = await engine.ReviewAsync(data.Items, EngineHost.From(ctx));
        return StepOutcome.Info("Apply.Review.Done", Json.Args(("reviewed", n)));
    }
}
