using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>
/// Modo sem confirmação: confirma a KB (<c>Confirmado KB</c>) só do que tem cartão da KB, proposta de correção, SEM alerta e sintaxe sem erro.
/// O que não passa (alerta, IA, isenção, pendente) fica para o consultor — é a parada da SKILL.md. Sem <c>AutoConfirm</c> não faz nada.
/// </summary>
public sealed class AutoConfirmStep() : IRunStep
{
    public const string AutoObs = "modo automático";

    public string Code => RemediationApplyStepCodes.AutoConfirm;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ProposeParameters>();
        if (!p.AutoConfirm)
            return StepOutcome.Done;
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, p.GroupKeys, i => i.Status == RemediationStatus.Pending);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        if (!data.Session.AutoMode)
            return StepOutcome.Stop("Apply.AutoModeRequired");
        var confirmed = 0;
        await ctx.MutateAsync(() =>
        {
            foreach (var i in data.Items.Where(Eligible))
            {
                var obs = $"KB {i.Proposal!.KbCardId} ({AutoObs})";
                i.MarkStatus(RemediationStatus.ConfirmedKb, obs, ctx.Clock.UtcNow);
                if (CorrectionSheet.FromProposal(i.Proposal) is { } sheet)
                    i.SetCorrectionSheet(CorrectionSheet.Render(i, RemediationStatus.ConfirmedKb, obs, sheet.Before, sheet.After, i.CorrectionSheetMd));
                confirmed++;
            }
        });
        await ctx.SaveAsync();
        return StepOutcome.Info("Apply.AutoConfirm.Done", Json.Args(("confirmed", confirmed), ("left", data.Items.Count - confirmed)));
    }

    public static bool Eligible(RemediationItem i) =>
        i.Proposal is { Action: ItemProposal.ActionFix, Origin: ItemProposal.OriginKb, KbCardId: not null, Alerts.Count: 0 } p && p.SyntaxOk != false
        && p.NewSource is not null && i.Preparation?.NewSource is not null;
}
