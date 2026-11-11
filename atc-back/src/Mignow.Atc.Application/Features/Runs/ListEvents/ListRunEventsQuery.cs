using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Runs.ListEvents;

/// <summary>Log da execução, do mais recente para o mais antigo.</summary>
public sealed record ListRunEventsQuery(Guid RunId, EventLevel? Level = null, string? ItemRef = null, int Page = 1, int PageSize = 100)
    : IQuery<Result<PagedResult<RunEventResponse>>>;
