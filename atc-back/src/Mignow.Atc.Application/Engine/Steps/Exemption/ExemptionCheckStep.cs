using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Exemption;

/// <summary>
/// Conferência somente leitura (porte do <c>conferir.ps1</c>): totais do usuário (<c>checkexemptionsview</c>; já deu 0 mesmo
/// com pedidos criados) e, por finding PEDIDO, o modelo de novo: a mensagem do SAP (ex.: "Exemption already requested") é a
/// situação; se vier modelo novo o finding segue sem isenção (conferir na transação ATC). Não altera os pedidos.
/// </summary>
public sealed class ExemptionCheckStep(IAtcExemptions exemptions) : IRunStep
{
    public string Code => ExemptionStepCodes.Check;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var batch = await ExemptionStepSupport.LoadBatchAsync(ctx);
        var totals = await exemptions.GetUserTotalsAsync(ctx.SapConnection, ctx.Ct);
        if (totals.IsFailure)
            return StepOutcome.Stop(totals.Error.Code, Json.Args(("message", totals.Error.Message)));
        var requested = ExemptionStepSupport.Scope(ctx, batch).Where(r => r.Status == ExemptionRequestStatus.Requested).ToList();
        var items = await ExemptionStepSupport.ItemsAsync(ctx, requested);
        List<object> findings = [];
        for (var n = 0; n < requested.Count; n++)
        {
            if (await ctx.IsCancellationRequestedAsync())
                return StepOutcome.Cancel;
            var r = requested[n];
            await ctx.ReportProgressAsync(n, requested.Count);
            var t = await exemptions.GetTemplateAsync(ctx.SapConnection, r.MarkerId, ctx.Ct);
            if (t.IsFailure)
                return StepOutcome.Stop(t.Error.Code, Json.Args(("message", t.Error.Message)));
            findings.Add(new
            {
                seq = r.Seq, itemNumber = items[r.ItemId].N, objectName = items[r.ItemId].ObjectName, markerId = r.MarkerId,
                situationCode = t.Value.HasProposal ? "Exemption.Check.StillNoExemption" : "Exemption.Check.SapMessage",
                sapMessage = t.Value.HasProposal ? null : t.Value.Message, type = t.Value.Type,
            });
        }
        await ctx.ReportProgressAsync(requested.Count, Math.Max(1, requested.Count));
        await ctx.MutateAsync(() => batch.Advance(ExemptionBatchStatus.Checked));
        await ctx.SetResultAsync(new
        {
            user = ctx.SapConnection.User, totals = totals.Value.Totals, findings = findings.Count, situations = findings,
            history = ExemptionStepSupport.History(ctx, batch, ExemptionStepSupport.PolicyOf(batch)),
        });
        await ctx.SaveAsync();
        await ctx.LogAsync(EventLevel.Info, "Exemption.Check.Done", Json.Args(("count", findings.Count)));
        return StepOutcome.Info("Exemption.Check.Done", Json.Args(("count", findings.Count)));
    }
}
