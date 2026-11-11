namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Parâmetros do run <c>Rollback</c>: restaura o backup do item.</summary>
public sealed record RollbackParameters(Guid SessionId, Guid ItemId);
