using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Itens que o lote automático já confirmou (<c>modo automático</c> na observação) e em que ponto da aplicação estão.</summary>
public static class AutoSelection
{
    /// <param name="reprocess">Itens com erro de gravação que o play reaplica (entram mesmo confirmados pelo consultor).</param>
    public static bool Confirmed(RemediationItem i, IReadOnlyCollection<Guid>? reprocess = null) =>
        (i.Status is RemediationStatus.ConfirmedKb or RemediationStatus.ConfirmedAi)
        && (i.Obs.Contains(AutoConfirmStep.AutoObs, StringComparison.Ordinal) || reprocess?.Contains(i.Guid) == true);

    public static bool NeedsSimulation(RemediationItem i) => NeedsSimulation(i, null);

    public static bool NeedsSimulation(RemediationItem i, IReadOnlyCollection<Guid>? reprocess) =>
        Confirmed(i, reprocess) && i.Application?.Result is not ("ATIVADO" or "ROLLBACK_ATIVADO");

    public static bool Simulated(RemediationItem i) => Simulated(i, null);

    public static bool Simulated(RemediationItem i, IReadOnlyCollection<Guid>? reprocess) => Confirmed(i, reprocess) && i.Application?.Result == "SIMULADO";

    public static bool ReviewBlocks(RemediationItem i) => Mignow.Atc.Application.Modules.Remediation.Apply.Ai.ReviewPrompts.VerdictOf(i.Proposal?.Review) == "bloquear";
}
