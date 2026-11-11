using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.Create;

/// <summary>A senha vai direto para o cofre; o cadastro guarda só a referência. <c>Host = sim://cenario</c> = simulador.</summary>
public sealed record CreateSapSystemCommand(
    Guid CustomerId, string Sid, string Client, string Host, int AdtPort, bool Https, string Language, string? CertThumbprint, bool IsProduction,
    ProjectType ProjectType, string Author, string? DefaultRequest, string? ExemptionApprover, string Username, string? Password)
    : ICommand<Result<SapSystemResponse>>, ISapSystemInput;
