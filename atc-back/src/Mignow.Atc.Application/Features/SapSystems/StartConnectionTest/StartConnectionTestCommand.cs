using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.SapSystems.StartConnectionTest;

/// <summary>Teste de conexão como execução (Run <c>ConnectionTest</c>): exemplo do motor; devolve 202 com o run.</summary>
public sealed record StartConnectionTestCommand(Guid Id) : ICommand<Result<RunResponse>>;
