using Mignow.Atc.Application.Features.SessionApply.Decide;

namespace Mignow.Atc.Api.Contracts;

/// <summary>Corpo de <c>POST sessions/{id}/proposals/decision</c>.</summary>
public sealed record SessionApplyDecisionRequest(
    IReadOnlyList<Guid> ItemIds, string Decision, string? Obs = null, IReadOnlyList<string>? Replaces = null, string? Reason = null, IReadOnlyList<DecisionFact>? Facts = null);
