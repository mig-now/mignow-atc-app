using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Api.Contracts;

public sealed record UpdateSapSystemRequest(
    string Sid, string Client, string Host, int AdtPort, bool Https, string Language, string? CertThumbprint, bool IsProduction,
    ProjectType ProjectType, string Author, string? DefaultRequest, string? ExemptionApprover, string Username, bool Active);
