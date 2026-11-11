namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// Teste de conexão: release (S4CORE), coleções do discovery, categoria do mandante (T000-CCCATEGORY; nula = ilegível) e o
/// thumbprint do certificado visto (para fixar no cadastro no primeiro teste).
/// </summary>
public sealed record SapSystemInfo(string Release, IReadOnlyList<string> Collections, string? ClientCategory, string? CertificateThumbprint);
