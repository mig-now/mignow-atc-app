using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Remediation;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>
/// 1º passo do play: objetos distintos (com código ABAP) dos itens pendentes escolhidos, no mesmo formato do plano de <c>ReadSources</c> — o passo
/// seguinte (<see cref="ReadSourcesFetchStep"/>) lê esses fontes. Standard só é listado (fica para o consultor). Idempotente.
/// </summary>
public sealed class AutoSourcesPlanStep : IRunStep
{
    public string Code => AutoRemediationStepCodes.Plan;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<AutoRemediationParameters>();
        var data = await AutoRemediationItems.LoadAsync(ctx, p);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var pending = data.Value.Selected.Where(i => i.Status == RemediationStatus.Pending).ToList();
        var objects = pending.Where(i => i.ObjectName.Length > 0 && i.ObjectType.Length > 0 && !StandardObjects.NoCodeTypes.Contains(i.ObjectType.ToUpperInvariant()))
            .GroupBy(i => (Type: i.ObjectType.ToUpperInvariant(), Name: i.ObjectName))
            .OrderBy(g => g.Key.Type, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Key.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ReadSourcesObject(g.Key.Type, g.Key.Name, g.Count())).ToList();
        var standard = objects.Where(o => StandardObjects.IsStandard(o.Name)).ToList();
        foreach (var o in standard)
            await ctx.LogAsync(EventLevel.Warning, "Remediation.StandardNotRead", Json.Args(("type", o.Type), ("name", o.Name), ("items", o.Items)));
        await ctx.MergeResultAsync(new
        {
            selected = data.Value.Selected.Count, selectedPending = pending.Count,
            plan = new ReadSourcesPlan(pending.Count, [.. objects.Except(standard)], standard),
        });
        await ctx.ReportProgressAsync(1, 1);
        return StepOutcome.Info("Remediation.ReadSourcesPlanned", Json.Args(("objects", objects.Count - standard.Count), ("items", pending.Count)));
    }
}
