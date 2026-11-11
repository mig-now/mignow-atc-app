namespace Mignow.Atc.Domain.Exemption;

/// <summary>Status por finding (textos exatos da skill atc-isencao).</summary>
public static class ExemptionRequestStatus
{
    public const string Simulated = "SIMULADO";
    public const string Blocked = "BLOQUEADO";
    public const string NoScope = "SEM_ESCOPO";
    public const string AlreadyRequested = "JA_PEDIDO";
    public const string Requested = "PEDIDO";
    public const string Info = "INFO";
    public const string Error = "ERRO";

    public static readonly IReadOnlyList<string> All = [Simulated, Blocked, NoScope, AlreadyRequested, Requested, Info, Error];

    public static bool IsValid(string? status) => status is not null && All.Contains(status, StringComparer.Ordinal);
}
