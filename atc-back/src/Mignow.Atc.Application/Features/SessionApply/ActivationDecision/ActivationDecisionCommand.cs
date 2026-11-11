using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.SessionApply.ActivationDecision;

/// <summary>Decisão após <c>ERRO_ATIVACAO</c>: <c>keepInactive</c> (o desenvolvedor corrige no SE80/ADT) ou <c>rollback</c> (Run <c>Rollback</c> com o backup).</summary>
public sealed record ActivationDecisionCommand(Guid SessionId, Guid ItemId, string Decision) : ICommand<Result<ActivationDecisionResponse>>;
