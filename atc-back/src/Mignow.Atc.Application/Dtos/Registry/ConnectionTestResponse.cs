namespace Mignow.Atc.Application.Dtos.Registry;

/// <summary>Teste de conexão: release, categoria do mandante (T000) e o certificado fixado (quando HTTPS).</summary>
public sealed record ConnectionTestResponse(bool Ok, string? Release, string? ClientCategory, string? CertThumbprint, string? MessageCode, string? Message,
    long LatencyMs, DateTimeOffset TestedAt);
