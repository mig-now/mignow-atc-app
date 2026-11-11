namespace Mignow.Atc.Domain.Scan;

/// <summary>Status de um lote (textos do <c>executar.ps1</c>); lote ainda não executado = status nulo.</summary>
public static class ScanBatchStatus
{
    public const string Ok = "OK";
    public const string LockError = "ERRO_LOCK";
    public const string WriteError = "ERRO_GRAVACAO";
    public const string ActivationError = "ERRO_ATIVACAO";
    public const string ExecutionError = "ERRO_EXECUCAO";
}
