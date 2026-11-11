using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Rollback com o backup guardado no item (<c>aplicar_adt -Rollback</c>): grava o backup de volta e ativa (<c>ROLLBACK_ATIVADO</c>).</summary>
public sealed class RollbackStep(ApplyEngine engine) : IRunStep
{
    public string Code => RemediationApplyStepCodes.Rollback;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<RollbackParameters>();
        var data = await StepData.LoadAsync(ctx, p.SessionId, [p.ItemId], null, _ => true);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var batch = await engine.RollbackAsync(data.ToApplyInput(ctx, false, null), EngineHost.From(ctx));
        await ctx.SetResultAsync(new { rollback = batch.Reports });
        await ctx.SaveAsync();
        if (batch.StopCode is not null)
            return StepOutcome.Stop(batch.StopCode);
        return StepOutcome.Info("Apply.Rollback.Done", Json.Args(("result", batch.Reports.FirstOrDefault()?.Result)));
    }
}
