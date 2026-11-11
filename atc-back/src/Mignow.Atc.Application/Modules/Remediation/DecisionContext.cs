using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Contexto entregue aos <see cref="IDecisionHook"/>: o item já marcado (status/obs gravados em memória), o status anterior e os parâmetros do pedido.</summary>
public sealed record DecisionContext(
    RemediationSession Session, RemediationItem Item, SessionDecision Decision, string PreviousStatus, string? Obs, IReadOnlyList<string> Replaces, string? Reason);
