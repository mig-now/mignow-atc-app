using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.Update;

/// <summary>A senha muda só pelo <c>PUT {id}/password</c>.</summary>
public sealed record UpdateSapSystemCommand(
    Guid Id, string Sid, string Client, string Host, int AdtPort, bool Https, string Language, string? CertThumbprint, bool IsProduction,
    ProjectType ProjectType, string Author, string? DefaultRequest, string? ExemptionApprover, string Username, bool Active)
    : ICommand<Result<SapSystemResponse>>, ISapSystemInput;
