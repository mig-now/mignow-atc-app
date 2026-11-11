using System.Security.Cryptography;
using System.Text;

namespace Mignow.Atc.Domain.ValueObjects;

/// <summary>Hash do fonte (fins de linha normalizados) para conferir se o objeto mudou depois da gravação.</summary>
public static class SourceHash
{
    public static string Of(string source) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source.Replace("\r\n", "\n").TrimEnd()))).ToLowerInvariant();
}
