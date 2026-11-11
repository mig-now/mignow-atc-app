using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Modo sem confirmação: simula o que o lote confirmou. Fonte mudou/erro de preparação = parada daquele item (fica para o consultor).</summary>
public sealed class AutoSimulateStep(ApplyEngine engine) : IRunStep
{
    public string Code => RemediationApplyStepCodes.AutoSimulate;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ProposeParameters>();
        if (!p.AutoConfirm)
            return StepOutcome.Done;
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, p.GroupKeys, i => AutoSelection.NeedsSimulation(i, p.Reprocess));
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var batch = await engine.SimulateAsync(data.ToApplyInput(ctx, false, null), EngineHost.From(ctx));
        return StepOutcome.Info("Apply.Simulate.Done", Json.Args(("items", batch.Reports.Count), ("simulated", batch.Reports.Count(r => r.Result == ApplyResults.Simulated))));
    }
}
