namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// Resultado da gravação com os códigos do <c>aplicar_adt.ps1</c>: <c>ATIVADO</c>, <c>ROLLBACK_ATIVADO</c>,
/// <c>ERRO_ATIVACAO</c>, <c>ERRO_ROLLBACK_ATIVACAO</c>, <c>ERRO_GRAVACAO</c>, <c>ERRO_LOCK</c>, <c>ERRO_REQUEST</c>, <c>ERRO</c>.
/// </summary>
public sealed record SourceWriteOutcome(
    string Result, string? Error, string? Observation, string? Unlock, string? ActivationContext, IReadOnlyList<string> Messages)
{
    public const string Activated = "ATIVADO";
    public const string RollbackActivated = "ROLLBACK_ATIVADO";
    public const string ActivationError = "ERRO_ATIVACAO";
    public const string RollbackActivationError = "ERRO_ROLLBACK_ATIVACAO";
    public const string WriteError = "ERRO_GRAVACAO";
    public const string LockError = "ERRO_LOCK";
    public const string RequestError = "ERRO_REQUEST";
    public const string GenericError = "ERRO";

    public bool IsActivated => Result is Activated or RollbackActivated;
}
