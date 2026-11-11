namespace Mignow.Atc.Infra.Database.Secrets;

/// <summary>Segredo cifrado (AES-256-GCM). Só o EncryptedSecretStore lê e grava; a chave fica fora do banco.</summary>
public sealed class StoredSecret
{
    public int Id { get; set; }

    public string Ref { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public byte[] Nonce { get; set; } = [];

    public byte[] Cipher { get; set; } = [];

    public byte[] Tag { get; set; } = [];

    public DateTimeOffset UpdatedAt { get; set; }
}
