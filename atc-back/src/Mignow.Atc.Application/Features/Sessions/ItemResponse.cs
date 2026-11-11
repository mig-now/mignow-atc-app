using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions;

/// <summary>
/// Item na lista (sem os blocos pesados). <c>ApplicationResult</c>/<c>ApplicationDetail</c> = resultado e detalhe da última tentativa no SAP
/// (o status "Confirmado" é só a decisão: a gravação pode ter falhado). <c>AwaitingApproval</c> = pendente de aprovação (já tem proposta
/// esperando a decisão do consultor); <c>ProposalNote</c>/<c>ProposalAction</c> = justificativa e sugestão da proposta
/// (<c>corrigir</c> | <c>isencao</c> | <c>cancelar</c> | <c>pendente</c>).
/// </summary>
public sealed record ItemResponse(
    Guid Id, int N, string ObjectName, string ObjectType, string? RefObject, string? RefType, string? Check, string? Message, string? Note, string? Text,
    string? Package, string? Classification, string? Priority, string? FindingId, string Origin, string Status, string Obs, DateTimeOffset? StatusDate,
    IReadOnlyList<string> TriageFlags, bool HasProposal, bool HasPreparation, string? ApplicationResult,
    string? ApplicationDetail, bool AwaitingApproval = false, string? ProposalNote = null,
    string? ProposalAction = null);

/// <summary>Item completo: histórico, preparação, proposta, aplicação, ficha e a linha original do CSV.</summary>
public sealed record ItemDetailResponse(
    ItemResponse Item, IReadOnlyList<StatusChange> StatusHistory, ItemPreparation? Preparation, ItemProposal? Proposal, ItemApplication? Application,
    string? CorrectionSheetMd, IReadOnlyList<string> RawRow);
