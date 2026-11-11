using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Runs.List;

/// <summary>Execuções (mais recentes primeiro), com filtros por ambiente, cliente, tipo, alvo só ativas e lista de situações (<c>status</c>: um ou mais valores).</summary>
public sealed record ListRunsQuery(
    Guid? SapSystemId = null, Guid? CustomerId = null, RunKind? Kind = null, Guid? SubjectId = null, bool ActiveOnly = false, int Page = 1,
    int PageSize = 20, IReadOnlyList<RunStatus>? Statuses = null) : IQuery<Result<PagedResult<RunResponse>>>;
