using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>
/// 1º passo de <c>ReadSources</c> (porte da seleção do <c>ler_fontes_lote.ps1</c>): os N primeiros pendentes da fila (cliente primeiro, standard no
/// fim), objetos distintos que têm código ABAP (tipos sem código ficam para a triagem R1) e, sem <c>IncludeStandard</c>, os standard só são listados.
/// Idempotente: só grava o plano no resultado do run.
/// </summary>
public sealed class ReadSourcesPlanStep : IRunStep
{
    public string Code => RemediationStepCodes.ReadSourcesPlan;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ReadSourcesParameters>();
        var items = await ctx.WithDbAsync(async () =>
        {
            var session = await ctx.Store.FirstOrDefaultAsync(ctx.Store.Read<RemediationSession>().Where(s => s.Guid == p.SessionId), ctx.Ct);
            if (session is null)
                return null;
            var id = session.Id;
            return await ctx.Store.ListAsync(ctx.Store.Read<RemediationItem>().Where(i => i.SessionId == id && i.Status == RemediationStatus.Pending).OrderBy(i => i.N), ctx.Ct);
        });
        if (items is null)
            return StepOutcome.Stop("Session.NotFound");
        var batch = p.Batch > 0 ? p.Batch : ctx.Settings.BatchSize;
        var queue = StandardObjects.PendingQueue(items).Take(batch).ToList();
        var objects = queue.Where(i => i.ObjectName.Length > 0 && i.ObjectType.Length > 0 && !StandardObjects.NoCodeTypes.Contains(i.ObjectType.ToUpperInvariant()))
            .GroupBy(i => (Type: i.ObjectType.ToUpperInvariant(), Name: i.ObjectName))
            .OrderBy(g => g.Key.Type, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Key.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ReadSourcesObject(g.Key.Type, g.Key.Name, g.Count())).ToList();
        var standard = objects.Where(o => StandardObjects.IsStandard(o.Name)).ToList();
        if (!p.IncludeStandard)
        {
            foreach (var o in standard)
                await ctx.LogAsync(EventLevel.Warning, "Remediation.StandardNotRead", Json.Args(("type", o.Type), ("name", o.Name), ("items", o.Items)));
            objects = [.. objects.Except(standard)];
        }
        else
            standard = [];
        await ctx.SetResultAsync(new { plan = new ReadSourcesPlan(queue.Count, objects, standard) });
        await ctx.ReportProgressAsync(1, 1);
        return StepOutcome.Info("Remediation.ReadSourcesPlanned", Json.Args(("objects", objects.Count), ("items", queue.Count)));
    }
}
