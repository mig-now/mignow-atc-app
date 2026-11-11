using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Features.Exemptions;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Exemption;

/// <summary>
/// Envio (GRAVA NO SAP; porte do <c>solicitar.ps1 -Confirmado</c>): só depois da simulação e só o que está SIMULADO (e tem
/// escopo). Para antes de enviar se a política não for <c>A</c>, se o aprovador for o solicitante ou se a trava de escrita
/// bloquear (produção, T000=P, categoria ilegível). Status por finding: PEDIDO | INFO | ERRO (BLOQUEADO/SEM_ESCOPO se o modelo mudou).
/// </summary>
public sealed class ExemptionSendStep(IAtcExemptions exemptions, ISapWriter writer) : IRunStep
{
    public string Code => ExemptionStepCodes.Send;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var guard = await writer.EnsureWriteAllowedAsync(ctx.SapConnection, ctx.Ct);
        if (guard.IsFailure)
            return StepOutcome.Stop(guard.Error.Code, Json.Args(("message", guard.Error.Message)));
        var batch = await ExemptionStepSupport.LoadBatchAsync(ctx);
        var approver = batch.Approver;
        if (string.IsNullOrWhiteSpace(approver))
            return StepOutcome.Stop("Exemption.ApproverRequired");
        if (ExemptionRules.IsRequester(approver, ctx.SapConnection.User))
            return StepOutcome.Stop("Exemption.ApproverIsRequester", Json.Args(("approver", approver.ToUpperInvariant())));
        var policy = await exemptions.GetPolicyAsync(ctx.SapConnection, ctx.Ct);
        if (policy.IsFailure)
            return StepOutcome.Stop(policy.Error.Code, Json.Args(("message", policy.Error.Message)));
        if (!policy.Value.AllowsLocal)
            return StepOutcome.Stop("Exemption.Send.PolicyDisabled", Json.Args(("value", policy.Value.Value), ("text", policy.Value.Text)));
        var customizing = await exemptions.GetCustomizingAsync(ctx.SapConnection, ctx.Ct);
        if (customizing.IsFailure)
            return StepOutcome.Stop(customizing.Error.Code, Json.Args(("message", customizing.Error.Message)));

        var todo = ExemptionStepSupport.Scope(ctx, batch).Where(r => r.Status == ExemptionRequestStatus.Simulated).ToList();
        var known = customizing.Value.Reasons;
        if (todo.FirstOrDefault(r => known.Count > 0 && known.All(k => k.Id != r.Reason)) is { } bad)
            return StepOutcome.Stop("Exemption.ReasonUnknown", Json.Args(("reason", bad.Reason), ("valid", string.Join(", ", known.Select(k => k.Id)))));

        for (var n = 0; n < todo.Count; n++)
        {
            if (await ctx.IsCancellationRequestedAsync())
                return StepOutcome.Cancel;
            var r = todo[n];
            await ctx.ReportProgressAsync(n, todo.Count);
            var sent = await exemptions.SubmitAsync(ctx.SapConnection, new ExemptionApply(r.MarkerId, approver, r.Reason, r.Justification, batch.Validity), ctx.Ct);
            if (sent.IsFailure && sent.Error.Type == ErrorType.Forbidden)
                return StepOutcome.Stop(sent.Error.Code, Json.Args(("message", sent.Error.Message)));
            await ctx.MutateAsync(() =>
            {
                if (sent.IsFailure)
                    r.Record(ExemptionRequestStatus.Error, sent.Error.Message);
                else
                    r.Record(sent.Value.Status, sent.Value.Message, sent.Value.RequestXml, sent.Value.ResponseXml);
            });
            await ctx.SaveAsync();
        }
        await ctx.ReportProgressAsync(todo.Count, Math.Max(1, todo.Count));
        await ctx.MutateAsync(() => batch.Advance(ExemptionBatchStatus.Sent));
        var summary = todo.GroupBy(r => r.Status ?? ExemptionStatusMath.PendingKey).ToDictionary(g => g.Key, g => g.Count());
        var messages = todo.Where(r => !string.IsNullOrEmpty(r.Message)).GroupBy(r => r.Message!).Select(g => $"{g.Count()} x {g.Key}").ToList();
        await ctx.SetResultAsync(new
        {
            mode = "send", approver = approver.ToUpperInvariant(), validity = batch.Validity, findings = todo.Count, summary, messages,
            history = ExemptionStepSupport.History(ctx, batch, policy.Value.Display),
        });
        await ctx.SaveAsync();
        await ctx.LogAsync(EventLevel.Info, "Exemption.Send.Done", Json.Args(("count", todo.Count)));
        return StepOutcome.Info("Exemption.Send.Done", Json.Args(("count", todo.Count)));
    }
}
