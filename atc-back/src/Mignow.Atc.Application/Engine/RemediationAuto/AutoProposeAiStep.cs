using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.RemediationApply;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>
/// Depois da proposta pela KB: o que continua pendente sem correção proposta (sem cartão na KB ou KB que não se aplicou) vai para a análise da IA
/// (prioridade KB, senão IA). Standard fica fora. Idempotente (reescreve a proposta dos que ainda estão sem correção).
/// </summary>
public sealed class AutoProposeAiStep(ProposeEngine engine, IDateTimeProvider clock) : IRunStep
{
    public string Code => AutoRemediationStepCodes.ProposeAi;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<AutoRemediationParameters>();
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, null, NeedsAi);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        if (data.Items.Count == 0)
            return StepOutcome.Info("AutoRemediation.NothingForAi");
        await ctx.SetParallelismAsync(ctx.Settings.Parallelism);
        var date = DateOnly.FromDateTime(clock.UtcNow.LocalDateTime);
        var summary = await engine.RunAsync(new ProposeInput(ProposeEngine.ModeAi, ChangeMarker.AuthorTag(data.Session.Author), date, data.Items, data.Caches, data.Columns, await data.LocationsAsync(ctx)),
            EngineHost.From(ctx));
        await ctx.MergeResultAsync(new { proposeAi = summary });
        await ctx.SaveAsync();
        return StepOutcome.Info("Apply.Propose.Done", Json.Args(("items", summary.Items), ("fix", summary.Fix), ("pending", summary.Pending)));
    }

    public static bool NeedsAi(RemediationItem i) =>
        i.Status == RemediationStatus.Pending && !AutoRemediationItems.IsStandard(i) && i.Proposal?.Action != ItemProposal.ActionFix;
}
