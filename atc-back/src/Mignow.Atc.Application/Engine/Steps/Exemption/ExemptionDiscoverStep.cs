using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Exemption;

/// <summary>
/// Descoberta somente leitura (porte do <c>descobrir.ps1</c>): serviço ADT, política (deve ser <c>A</c>), motivos/validades,
/// modelo do 1º finding do lote, aprovadores. Guarda tudo em <c>DiscoveryJson</c>; com bloqueio (política ≠ A, serviço
/// ausente, sem modelo) a execução para com a mensagem da skill e o lote não avança.
/// </summary>
public sealed class ExemptionDiscoverStep(IAtcExemptions exemptions) : IRunStep
{
    public string Code => ExemptionStepCodes.Discover;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var batch = await ExemptionStepSupport.LoadBatchAsync(ctx);
        await ctx.ReportProgressAsync(0, 3);
        var found = await exemptions.DiscoverAsync(ctx.SapConnection, ctx.Ct);
        if (found.IsFailure)
            return StepOutcome.Stop(found.Error.Code, Json.Args(("message", found.Error.Message)));
        var d = found.Value;
        List<(string Code, string Args)> blockers = [], warnings = [];
        if (!d.ApplyPublished)
            blockers.Add(("Exemption.Discovery.ApplyNotPublished", "{}"));
        if (!d.Policy.AllowsLocal)
            blockers.Add(("Exemption.Discovery.PolicyNotAllowed", Json.Args(("value", d.Policy.Value), ("text", d.Policy.Text))));
        await ctx.ReportProgressAsync(1, 3);

        object? model = null;
        var first = batch.Requests.OrderBy(r => r.Seq).FirstOrDefault();
        if (first is not null)
        {
            var t = await exemptions.GetTemplateAsync(ctx.SapConnection, first.MarkerId, ctx.Ct);
            if (t.IsFailure)
                return StepOutcome.Stop(t.Error.Code, Json.Args(("message", t.Error.Message)));
            model = new { markerId = first.MarkerId, http = t.Value.Http, hasModel = t.Value.HasProposal, message = t.Value.Message, type = t.Value.Type };
            if (!t.Value.HasProposal)
                blockers.Add(("Exemption.Discovery.NoTemplate", Json.Args(("message", t.Value.Message))));
        }
        await ctx.ReportProgressAsync(2, 3);
        if (d.Approvers == 0 && d.Areas == 0)
            (d.Policy.AllowsLocal ? warnings : blockers).Add(("Exemption.Discovery.NoApprovers", "{}"));

        var discovery = new
        {
            ok = blockers.Count == 0,
            policy = new { value = d.Policy.Value, keyExists = d.Policy.KeyExists, text = d.Policy.Text, allowsLocal = d.Policy.AllowsLocal, display = d.Policy.Display },
            adtApply = d.ApplyPublished, adtCollections = d.Collections, reasons = d.Customizing.Reasons, validities = d.Customizing.Validities,
            userExemptions = d.UserTotal, approvers = d.Approvers, approvalAreas = d.Areas, odata = d.ODataServices, rfc = d.RfcFunctions, model,
            blockers = blockers.Select(b => new { code = b.Code, args = b.Args }), warnings = warnings.Select(w => new { code = w.Code, args = w.Args }),
        };
        await ctx.MutateAsync(() =>
        {
            batch.SetDiscovery(Json.Write(discovery));
            if (blockers.Count == 0)
                batch.Advance(ExemptionBatchStatus.Discovered);
        });
        await ctx.SetResultAsync(new { discovery.ok, discovery.policy, discovery.blockers, discovery.warnings, history = ExemptionStepSupport.History(ctx, batch, d.Policy.Display) });
        await ctx.SaveAsync();
        await ctx.ReportProgressAsync(3, 3);
        if (blockers.Count > 0)
        {
            await ctx.LogAsync(EventLevel.Error, blockers[0].Code, blockers[0].Args);
            return StepOutcome.Stop(blockers[0].Code, blockers[0].Args);
        }
        foreach (var w in warnings)
            await ctx.LogAsync(EventLevel.Warning, w.Code, w.Args);
        return StepOutcome.Info("Exemption.Discovery.Ok", Json.Args(("policy", d.Policy.Display)));
    }
}
