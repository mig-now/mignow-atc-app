using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Extractions.Start;

/// <summary>Extrai a última execução concluída do ATC do ambiente (Run <c>Extraction</c>, 202). <see cref="Force"/> = extrai de novo uma já extraída.</summary>
public sealed record StartExtractionCommand(Guid SapSystemId, bool Force) : ICommand<Result<RunResponse>>;
