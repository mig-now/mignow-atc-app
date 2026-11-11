namespace Mignow.Atc.Infra.External.Identity;

/// <summary>Usuário de desenvolvimento (só Development/Local sem Entra configurado): requisições anônimas viram este usuário.</summary>
public sealed class DevAuthOptions
{
    public bool Enabled { get; set; }

    public string Email { get; set; } = "dev@migatc.local";

    public string Name { get; set; } = "Consultor Dev";
}
