using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Extractions.Check;

/// <summary>Verificação da última execução do ATC como execução (Run <c>ExtractionCheck</c>, 202); o resultado fica no run.</summary>
public sealed record StartExtractionCheckCommand(Guid SapSystemId) : ICommand<Result<RunResponse>>;
