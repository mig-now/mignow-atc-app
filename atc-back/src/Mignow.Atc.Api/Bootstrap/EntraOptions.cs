namespace Mignow.Atc.Api.Bootstrap;

/// <summary>Seção <c>Entra</c>. GUIDs vazios, placeholders 00000000-… ou CHANGE_ME = não configurado.</summary>
public sealed class EntraOptions
{
    public const string Section = "Entra";

    public string? TenantId { get; set; }

    public string? ClientId { get; set; }

    public bool IsConfigured => Real(TenantId) && Real(ClientId);

    private static bool Real(string? value) =>
        !string.IsNullOrWhiteSpace(value) && !value.StartsWith("00000000-", StringComparison.Ordinal) && !value.Contains("CHANGE_ME", StringComparison.Ordinal);
}
