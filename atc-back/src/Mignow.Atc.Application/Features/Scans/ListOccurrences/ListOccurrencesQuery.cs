using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.ListOccurrences;

/// <summary>Ocorrências paginadas; filtros: regra, cruzamento (<c>ATC_PEGOU</c>…), situação (P/R/I) e parte do nome do objeto.</summary>
public sealed record ListOccurrencesQuery(Guid Id, string? RuleId, string? Cross, string? Status, string? ObjectName, int Page, int PageSize)
    : IQuery<Result<PagedResult<ScanOccurrenceResponse>>>;
