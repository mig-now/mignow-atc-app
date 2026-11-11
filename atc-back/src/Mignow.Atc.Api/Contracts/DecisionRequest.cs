using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Api.Contracts;

public sealed record DecisionRequest(IReadOnlyList<Guid> ItemIds, SessionDecision Decision, string? Obs = null, IReadOnlyList<string>? Replaces = null, string? Reason = null);
