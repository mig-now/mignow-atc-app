using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Runs.ListAiCalls;

/// <summary>Chamadas de IA da execução (uma linha por chamada, inclusive as que falharam).</summary>
public sealed record ListRunAiCallsQuery(Guid RunId, int Page = 1, int PageSize = 100) : IQuery<Result<PagedResult<RunAiCallResponse>>>;
