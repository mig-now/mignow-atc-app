using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.SessionApply.GetProposal;

/// <summary>
/// Proposta do item para a tela de aprovação: a proposta (blocos, origem, cartão da KB, justificativa), o diff, a revisão por IA (com o veredito),
/// os alertas, a sintaxe, os fatos candidatos à KB sugeridos pela IA (<c>FactCandidates</c>; também dentro de <c>Proposal</c>), o que foi preparado (inclusive objetos relacionados) e a aplicação (resultado, request, histórico).
/// </summary>
public sealed record ProposalResponse(
    Guid ItemId, int N, string Status, ItemProposal? Proposal, string? Diff, string? Review, string? ReviewVerdict, IReadOnlyList<string> Alerts, bool? SyntaxOk,
    ItemPreparation? Preparation, ItemApplication? Application, IReadOnlyList<ProposalFactCandidate> FactCandidates);
