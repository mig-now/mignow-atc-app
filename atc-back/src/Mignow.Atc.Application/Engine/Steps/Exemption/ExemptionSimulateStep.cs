using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Application.Features.Exemptions;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.Steps.Exemption;

/// <summary>
/// Simulação somente leitura (porte do <c>solicitar.ps1 -Simular</c>): por finding, justificativa (prefixo do lote + MIG-NOW
/// + obs do item), modelo do SAP e o XML que seria enviado. Status SIMULADO | BLOQUEADO | SEM_ESCOPO | JA_PEDIDO | ERRO.
/// Pedido já enviado neste lote fica como está; pedido de outro lote do mesmo ambiente = JA_PEDIDO.
/// </summary>
public sealed class ExemptionSimulateStep(IAtcExemptions exemptions) : IRunStep
{
    public string Code => ExemptionStepCodes.Simulate;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var batch = await ExemptionStepSupport.LoadBatchAsync(ctx);
        var approver = batch.Approver;
        if (string.IsNullOrWhiteSpace(approver))
            return StepOutcome.Stop("Exemption.ApproverRequired");
        var reason = batch.Reason;
        var customizing = await exemptions.GetCustomizingAsync(ctx.SapConnection, ctx.Ct);
        if (customizing.IsFailure)
            return StepOutcome.Stop(customizing.Error.Code, Json.Args(("message", customizing.Error.Message)));
        var reasons = customizing.Value.Reasons;
        if (reasons.Count > 0 && reasons.All(r => r.Id != reason))
            return StepOutcome.Stop("Exemption.ReasonUnknown", Json.Args(("reason", reason), ("valid", string.Join(", ", reasons.Select(r => r.Id)))));

        var scope = ExemptionStepSupport.Scope(ctx, batch);
        var items = await ExemptionStepSupport.ItemsAsync(ctx, scope);
        var author = await ctx.WithDbAsync(async () =>
        {
            var sid = batch.SessionId;
            return await ctx.Store.FirstOrDefaultAsync(ctx.Store.Read<RemediationSession>().Where(s => s.Id == sid).Select(s => s.Author), CancellationToken.None) ?? string.Empty;
        });
        var others = await ctx.WithDbAsync(async () =>
        {
            var sys = batch.SapSystemId;
            var bid = batch.Id;
            var markers = scope.Select(r => r.MarkerId).ToArray();
            return (await ctx.Store.ListAsync(ctx.Store.Read<ExemptionRequest>()
                .Where(r => r.Status == ExemptionRequestStatus.Requested && r.BatchId != bid && markers.Contains(r.MarkerId)
                            && ctx.Store.Read<ExemptionBatch>().Any(b => b.Id == r.BatchId && b.SapSystemId == sys))
                .Select(r => new { r.MarkerId, At = r.UpdatedAt ?? r.CreatedAt }), CancellationToken.None))
                .GroupBy(r => r.MarkerId).ToDictionary(g => g.Key, g => g.Max(x => x.At));
        });

        var today = ctx.Clock.UtcNow.ToLocalTime();
        string? example = null;
        for (var n = 0; n < scope.Count; n++)
        {
            if (await ctx.IsCancellationRequestedAsync())
                return StepOutcome.Cancel;
            var r = scope[n];
            await ctx.ReportProgressAsync(n, scope.Count);
            if (r.Status == ExemptionRequestStatus.Requested)
                continue;
            if (others.TryGetValue(r.MarkerId, out var when))
            {
                await ctx.MutateAsync(() => r.Record(ExemptionRequestStatus.AlreadyRequested, $"pedido em {when:s}"));
                await ctx.SaveAsync();
                continue;
            }
            var justification = ExemptionRules.Justification(batch, reason, items[r.ItemId].Obs, author, today);
            example ??= justification;
            await ctx.MutateAsync(() => r.Prepare(reason, justification));
            var preview = await exemptions.PreviewAsync(ctx.SapConnection, new ExemptionApply(r.MarkerId, approver, reason, justification, batch.Validity), ctx.Ct);
            await ctx.MutateAsync(() =>
            {
                if (preview.IsFailure)
                    r.Record(ExemptionRequestStatus.Error, preview.Error.Message);
                else
                    r.Record(preview.Value.Status, preview.Value.Message, preview.Value.RequestXml);
            });
            await ctx.SaveAsync();
        }
        await ctx.ReportProgressAsync(scope.Count, Math.Max(1, scope.Count));
        await ctx.MutateAsync(() => batch.Advance(ExemptionBatchStatus.Simulated));
        var summary = scope.GroupBy(r => r.Status ?? ExemptionStatusMath.PendingKey).ToDictionary(g => g.Key, g => g.Count());
        var messages = scope.Where(r => !string.IsNullOrEmpty(r.Message)).GroupBy(r => r.Message!).Select(g => $"{g.Count()} x {g.Key}").ToList();
        await ctx.SetResultAsync(new
        {
            mode = "simulation", approver = approver.ToUpperInvariant(), reason, validity = batch.Validity, findings = scope.Count, summary, messages,
            exampleJustification = example, history = ExemptionStepSupport.History(ctx, batch, ExemptionStepSupport.PolicyOf(batch)),
        });
        await ctx.SaveAsync();
        await ctx.LogAsync(EventLevel.Info, "Exemption.Simulate.Done", Json.Args(("count", scope.Count)));
        return StepOutcome.Info("Exemption.Simulate.Done", Json.Args(("count", scope.Count)));
    }
}
