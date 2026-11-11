using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>
/// Modo sem confirmação: aplica no SAP (em fila, request da sessão) o que a simulação aprovou e a revisão não bloqueou. Erro de ativação, de gravação ou
/// de lock deixa o item para o consultor (sem rollback automático: a decisão manter inativa/rollback é dele).
/// </summary>
public sealed class AutoApplyStep(ApplyEngine engine) : IRunStep
{
    public string Code => RemediationApplyStepCodes.AutoApply;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ProposeParameters>();
        if (!p.AutoConfirm)
            return StepOutcome.Done;
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, p.GroupKeys, i => AutoSelection.Simulated(i, p.Reprocess) && !AutoSelection.ReviewBlocks(i));
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var batch = await engine.ApplyAsync(data.ToApplyInput(ctx, false, null), EngineHost.From(ctx));
        await ctx.MergeResultAsync(new { apply = batch.Reports });
        await ctx.SaveAsync();
        if (batch.StopCode is not null)
            return StepOutcome.Stop(batch.StopCode);
        return StepOutcome.Info("Apply.Apply.Done", Json.Args(("items", batch.Reports.Count), ("activated", batch.Reports.Count(r => r.Result == ApplyResults.Activated))));
    }
}
