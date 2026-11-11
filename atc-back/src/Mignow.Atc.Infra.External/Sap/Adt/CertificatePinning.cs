using System.Collections.Concurrent;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Validação TLS das chamadas ADT (porte do <c>AdtCertPin</c> da skill): certificado válido de CA passa; senão, só o
/// thumbprint fixado no ambiente (<c>CertThumbprint</c>: SHA-1 de 40 hex como o Windows, ou SHA-256 de 64 hex). Sem
/// thumbprint fixado, só o teste de conexão aceita o primeiro certificado visto (trust on first use) e o registra em
/// <see cref="LastSeen"/> para o cadastro fixar. O handler valida por conexão: a regra de cada host é registrada pela sessão.
/// </summary>
public static class CertificatePinning
{
    private static readonly ConcurrentDictionary<string, (string? Pin, bool TrustOnFirstUse)> Hosts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, string> Seen = new(StringComparer.OrdinalIgnoreCase);

    public static void Register(string host, string? pinnedThumbprint, bool trustOnFirstUse)
    {
        var pin = string.IsNullOrWhiteSpace(pinnedThumbprint) ? null : pinnedThumbprint.Replace(":", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
        Hosts.AddOrUpdate(host, (pin, trustOnFirstUse && pin is null),
            (_, current) => (current.Pin ?? pin, (current.TrustOnFirstUse || trustOnFirstUse) && (current.Pin ?? pin) is null));
    }

    /// <summary>Thumbprint SHA-1 (maiúsculas) do último certificado visto no host; nulo se nenhum.</summary>
    public static string? LastSeen(string host) => Seen.GetValueOrDefault(host);

    public static bool ValidateConnection(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
    {
        var host = (sender as SslStream)?.TargetHostName ?? string.Empty;
        if (certificate is not null)
            Seen[host] = Convert.ToHexString(SHA1.HashData(certificate.GetRawCertData()));
        if (errors == SslPolicyErrors.None)
            return true;
        if (certificate is null || !Hosts.TryGetValue(host, out var rule))
            return false;
        if (rule.Pin is not null)
            return Matches(certificate.GetRawCertData(), rule.Pin);
        return rule.TrustOnFirstUse;
    }

    internal static bool Matches(byte[] rawCertificate, string pin) => pin.Length switch
    {
        40 => string.Equals(Convert.ToHexString(SHA1.HashData(rawCertificate)), pin, StringComparison.OrdinalIgnoreCase),
        64 => string.Equals(Convert.ToHexString(SHA256.HashData(rawCertificate)), pin, StringComparison.OrdinalIgnoreCase),
        _ => false,
    };
}
