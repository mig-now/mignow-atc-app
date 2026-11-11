using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>Último passo do play: situação final dos itens escolhidos (para o monitor e a exportação). Só lê.</summary>
public sealed class AutoRemediationSummaryStep : IRunStep
{
    public string Code => AutoRemediationStepCodes.Summary;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<AutoRemediationParameters>();
        var data = await AutoRemediationItems.LoadAsync(ctx, p);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var items = data.Value.Selected;
        var summary = new AutoRemediationSummary(
            items.Count,
            items.Count(i => i.Status == RemediationStatus.ConfirmedKb),
            items.Count(i => i.Status == RemediationStatus.ConfirmedAi),
            items.Count(i => i.Status == RemediationStatus.Exemption),
            items.Count(i => i.Status == RemediationStatus.Cancelled),
            items.Count(i => i.Status == RemediationStatus.Pending),
            items.Count(i => i.Application?.Result == ApplyResults.Activated),
            items.Count(i => i.Application?.Result is { } r && (r.StartsWith("ERRO", StringComparison.Ordinal) || r.StartsWith("ABORTADO", StringComparison.Ordinal))),
            items.Count(AutoRemediationItems.IsStandard),
            items.Count(i => i.Status == RemediationStatus.Pending && i.AwaitingApproval));
        await ctx.MergeResultAsync(new { summary });
        await ctx.ReportProgressAsync(1, 1);
        return StepOutcome.Info("AutoRemediation.Done", Json.Args(("items", summary.Items), ("activated", summary.Activated), ("pending", summary.Pending)));
    }
}

