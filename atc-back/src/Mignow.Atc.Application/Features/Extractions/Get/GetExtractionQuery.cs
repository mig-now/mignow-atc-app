using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;

namespace Mignow.Atc.Application.Features.Extractions.Get;

public sealed record GetExtractionQuery(Guid Id) : IQuery<Result<ExtractionResponse>>;
