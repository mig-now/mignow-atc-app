using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.TestConnection;

/// <summary>
/// Teste síncrono (como o <c>testar_conexao</c>): VPN + credencial + discovery, release e T000-CCCATEGORY; grava o
/// resultado no cadastro e fixa o certificado visto quando ainda não há um.
/// </summary>
public sealed record TestSapConnectionCommand(Guid Id) : ICommand<Result<ConnectionTestResponse>>;
