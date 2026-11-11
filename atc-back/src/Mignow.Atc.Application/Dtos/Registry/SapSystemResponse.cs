using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Dtos.Registry;

/// <summary>Ambiente SAP. A senha nunca volta: só <see cref="HasPassword"/>.</summary>
public sealed record SapSystemResponse(
    Guid Id, Guid CustomerId, string CustomerName, string DisplayName, string Sid, string Client, string Host, int AdtPort, bool Https,
    string Language, string? CertThumbprint, bool IsProduction, ProjectType ProjectType, string Author, string? DefaultRequest,
    string? ExemptionApprover, string Username, bool HasPassword, bool Active, bool IsSimulator, string? Release, string? ClientCategory,
    DateTimeOffset? LastConnectionTestAt, bool? LastConnectionTestOk, string? LastConnectionTestMessage);
