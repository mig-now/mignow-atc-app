using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Features.Runs.Cancel;
using Mignow.Atc.Application.Features.Runs.Get;
using Mignow.Atc.Application.Features.Runs.List;
using Mignow.Atc.Application.Features.Runs.ListAiCalls;
using Mignow.Atc.Application.Features.Runs.ListEvents;
using Mignow.Atc.Application.Features.Runs.Retry;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Api.Controllers;

/// <summary>Execuções de todos os módulos (monitor, histórico, interromper, reprocessar).</summary>
[Route("v1/runs")]
[Tags("Runs")]
public sealed class RunsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult<RunResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RunResponse>>>> List(
        [FromServices] IQueryHandler<ListRunsQuery, Result<PagedResult<RunResponse>>> handler,
        [FromQuery] Guid? sapSystemId = null, [FromQuery] Guid? customerId = null, [FromQuery] RunKind? kind = null, [FromQuery] Guid? subjectId = null,
        [FromQuery] bool activeOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] RunStatus[]? status = null,
        CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListRunsQuery(sapSystemId, customerId, kind, subjectId, activeOnly, page, pageSize, status), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Get(Guid id,
        [FromServices] IQueryHandler<GetRunQuery, Result<RunResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetRunQuery(id), ct));

    [HttpGet("{id:guid}/events")]
    [ProducesResponseType<ApiResponse<PagedResult<RunEventResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<RunEventResponse>>>> Events(Guid id,
        [FromServices] IQueryHandler<ListRunEventsQuery, Result<PagedResult<RunEventResponse>>> handler,
        [FromQuery] EventLevel? level = null, [FromQuery] string? itemRef = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 100,
        CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListRunEventsQuery(id, level, itemRef, page, pageSize), ct));

    [HttpGet("{id:guid}/ai-calls")]
    [ProducesResponseType<ApiResponse<PagedResult<RunAiCallResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<RunAiCallResponse>>>> AiCalls(Guid id,
        [FromServices] IQueryHandler<ListRunAiCallsQuery, Result<PagedResult<RunAiCallResponse>>> handler,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListRunAiCallsQuery(id, page, pageSize), ct));

    /// <summary>Interromper: para num ponto seguro (nunca entre lock e unlock); na fila, cancela na hora.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Cancel(Guid id,
        [FromServices] ICommandHandler<CancelRunCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new CancelRunCommand(id), ct));

    /// <summary>Reprocessar uma execução com falha a partir do passo que parou.</summary>
    [HttpPost("{id:guid}/retry")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Retry(Guid id,
        [FromServices] ICommandHandler<RetryRunCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new RetryRunCommand(id), ct));
}
