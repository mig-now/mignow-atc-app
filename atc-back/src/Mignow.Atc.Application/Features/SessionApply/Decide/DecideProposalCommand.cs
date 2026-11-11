using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.SessionApply.Decide;

/// <summary>
/// Decisão do consultor sobre a proposta: <c>ConfirmKb</c> (aceita o cartão da KB) ou <c>ConfirmIa</c> (aceita a análise da IA e a grava na KB como
/// contribuição <c>C-&lt;ELEM&gt;-NNN</c>; <c>Replaces</c>/<c>Reason</c> = cartões da KB que ela substitui e por quê). <c>Facts</c> viram fatos da KB.
/// </summary>
public sealed record DecideProposalCommand(
    Guid SessionId, IReadOnlyList<Guid> ItemIds, string Decision, string? Obs, IReadOnlyList<string>? Replaces, string? Reason, IReadOnlyList<DecisionFact>? Facts)
    : ICommand<Result<DecideProposalResponse>>;
