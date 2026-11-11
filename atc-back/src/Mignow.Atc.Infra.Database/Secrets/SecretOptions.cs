namespace Mignow.Atc.Infra.Database.Secrets;

/// <summary>Seção <c>Security</c>. A chave (base64 de 32 bytes) vem de user-secrets ou variável de ambiente, nunca do JSON.</summary>
public sealed class SecretOptions
{
    public const string Section = "Security";

    public string? SecretKey { get; set; }

    public bool TryGetKey(out byte[] key)
    {
        key = [];
        if (string.IsNullOrWhiteSpace(SecretKey) || SecretKey.Contains("CHANGE_ME", StringComparison.Ordinal))
            return false;
        try
        {
            key = Convert.FromBase64String(SecretKey);
            return key.Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
