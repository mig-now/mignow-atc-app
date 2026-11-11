using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;

namespace Mignow.Atc.Application.Features.Extractions.ListFindings;

/// <summary>Findings da extração (por Seq): filtros prioridade (1..3), objeto (contém), baseline; <c>IncludeColumns</c> traz todas as colunas.</summary>
public sealed record ListFindingsQuery(Guid ExtractionId, int? Priority, string? Object, bool? InBaseline, bool IncludeColumns, int Page, int PageSize)
    : IQuery<Result<PagedResult<FindingResponse>>>;
