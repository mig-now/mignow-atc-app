using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.SessionApply.ActivationDecision;

/// <summary>Decisão gravada; <see cref="Run"/> só no rollback (202).</summary>
public sealed record ActivationDecisionResponse(string Decision, RunResponse? Run);
