using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.RemediationApply;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>
/// Confirmação automática do play (decisão do consultor, 09.11.2026): <c>Confirmado KB</c> o que tem cartão da KB, correção, sem alerta e sintaxe
/// sem erro (mesma regra do modo sem confirmação); <c>Confirmado IA</c> a correção da IA só com sintaxe verificada ok e sem alerta. O resto
/// (alerta/dúvida de negócio, isenção ou cancelamento sugeridos, falha) fica pendente para o consultor. A confirmação da IA aqui não grava cartão
/// novo na KB: o conhecimento só entra na KB pela decisão do consultor. Standard nunca é confirmado. A revisão por IA, depois da simulação, ainda
/// pode segurar o item.
/// </summary>
public sealed class AutoRemediationConfirmStep : IRunStep
{
    public string Code => AutoRemediationStepCodes.Confirm;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<AutoRemediationParameters>();
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, null, i => i.Status == RemediationStatus.Pending && !AutoRemediationItems.IsStandard(i));
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        int kb = 0, ai = 0;
        await ctx.MutateAsync(() =>
        {
            foreach (var i in data.Items)
            {
                string status, obs;
                if (AutoConfirmStep.Eligible(i))
                {
                    (status, obs) = (RemediationStatus.ConfirmedKb, $"KB {i.Proposal!.KbCardId} ({AutoConfirmStep.AutoObs})");
                    kb++;
                }
                else if (EligibleAi(i))
                {
                    (status, obs) = (RemediationStatus.ConfirmedAi, $"IA ({AutoConfirmStep.AutoObs})");
                    ai++;
                }
                else
                    continue;
                i.MarkStatus(status, obs, ctx.Clock.UtcNow);
                if (CorrectionSheet.FromProposal(i.Proposal) is { } sheet)
                    i.SetCorrectionSheet(CorrectionSheet.Render(i, status, obs, sheet.Before, sheet.After, i.CorrectionSheetMd));
            }
        });
        await ctx.SaveAsync();
        await ctx.MergeResultAsync(new { confirm = new { kb, ai, left = data.Items.Count - kb - ai } });
        return StepOutcome.Info("AutoRemediation.Confirmed", Json.Args(("kb", kb), ("ai", ai), ("left", data.Items.Count - kb - ai)));
    }

    /// <summary>Trava da IA: correção proposta, sintaxe verificada sem erro (<c>true</c>, não "não verificada"), sem alerta e fonte novo preparado.</summary>
    public static bool EligibleAi(RemediationItem i) =>
        i.Proposal is { Action: ItemProposal.ActionFix, Origin: ItemProposal.OriginAi, Alerts.Count: 0, SyntaxOk: true } p && p.NewSource is not null
        && i.Preparation?.NewSource is not null;
}
