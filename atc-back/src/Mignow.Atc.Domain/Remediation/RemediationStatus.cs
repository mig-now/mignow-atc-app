namespace Mignow.Atc.Domain.Remediation;

/// <summary>Textos exatos do status do item na skill (<c>STATUS_VALIDOS</c> do atc.py + vazio = pendente).</summary>
public static class RemediationStatus
{
    public const string Pending = "";
    public const string ConfirmedKb = "Confirmado KB";
    public const string ConfirmedAi = "Confirmado IA";
    public const string Cancelled = "Cancelado";
    public const string Exemption = "Isenção";

    public static readonly IReadOnlyList<string> All = [Pending, ConfirmedKb, ConfirmedAi, Cancelled, Exemption];

    public static bool IsValid(string? status) => status is not null && All.Contains(status, StringComparer.Ordinal);
}
