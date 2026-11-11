using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>1º passo de <c>Propose</c>: itens pendentes → proposta (KB/IA), marcadores, fonte novo, diff e sintaxe sem gravar. Idempotente (reescreve a proposta).</summary>
public sealed class ProposeStep(ProposeEngine engine, IDateTimeProvider clock) : IRunStep
{
    public string Code => RemediationApplyStepCodes.Propose;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ProposeParameters>();
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, p.GroupKeys, i => i.Status == RemediationStatus.Pending);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        if (data.Items.Count == 0)
            return StepOutcome.Info("Apply.Propose.NothingSelected");
        await ctx.SetParallelismAsync(ctx.Settings.Parallelism);
        var date = DateOnly.FromDateTime(clock.UtcNow.LocalDateTime);
        var summary = await engine.RunAsync(new ProposeInput(p.Mode, ChangeMarker.AuthorTag(data.Session.Author), date, data.Items, data.Caches, data.Columns, await data.LocationsAsync(ctx)), EngineHost.From(ctx));
        await ctx.MergeResultAsync(new { propose = summary });
        await ctx.SaveAsync();
        return StepOutcome.Info("Apply.Propose.Done", Json.Args(("items", summary.Items), ("fix", summary.Fix), ("pending", summary.Pending)));
    }
}
