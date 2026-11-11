using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;

namespace Mignow.Atc.Application.Features.Extractions.List;

/// <summary>Extrações gravadas (mais recentes primeiro), por ambiente (opcional).</summary>
public sealed record ListExtractionsQuery(Guid? SapSystemId, int Page, int PageSize) : IQuery<Result<PagedResult<ExtractionResponse>>>;
