using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Runs.Retry;

/// <summary>Reprocessar uma execução com falha a partir do passo que falhou (os concluídos não rodam de novo).</summary>
public sealed record RetryRunCommand(Guid RunId) : ICommand<Result<RunResponse>>;
