using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Simulação (<c>aplicar_adt -Simular</c>): confere a versão ativa, guarda o backup no item e valida o fonte novo. NÃO grava no SAP.</summary>
public sealed class SimulateStep(ApplyEngine engine) : IRunStep
{
    public string Code => RemediationApplyStepCodes.Simulate;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ApplyParameters>();
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, null, i => i.Status is RemediationStatus.ConfirmedKb or RemediationStatus.ConfirmedAi);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var batch = await engine.SimulateAsync(data.ToApplyInput(ctx, p.AllowTableStructure, p.DdicCreate), EngineHost.From(ctx));
        await ctx.SetResultAsync(new { simulate = batch.Reports });
        return StepOutcome.Info("Apply.Simulate.Done", Json.Args(("items", batch.Reports.Count), ("simulated", batch.Reports.Count(r => r.Result == ApplyResults.Simulated))));
    }
}
