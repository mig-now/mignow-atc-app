using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Aplicação no SAP, objeto por objeto em fila, com a request da sessão. Só itens Confirmado KB/IA com proposta preparada. Idempotente.</summary>
public sealed class ApplyStep(ApplyEngine engine) : IRunStep
{
    public string Code => RemediationApplyStepCodes.Apply;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ApplyParameters>();
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, null, i => i.Status is RemediationStatus.ConfirmedKb or RemediationStatus.ConfirmedAi);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var batch = await engine.ApplyAsync(data.ToApplyInput(ctx, p.AllowTableStructure, p.DdicCreate), EngineHost.From(ctx));
        await ctx.SetResultAsync(new { apply = batch.Reports });
        await ctx.SaveAsync();
        if (batch.StopCode is not null)
            return StepOutcome.Stop(batch.StopCode);
        return StepOutcome.Info("Apply.Apply.Done", Json.Args(("items", batch.Reports.Count), ("activated", batch.Reports.Count(r => r.Result == ApplyResults.Activated))));
    }
}
