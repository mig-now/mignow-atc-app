using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Features.Exemptions.Check;
using Mignow.Atc.Application.Features.Exemptions.Create;
using Mignow.Atc.Application.Features.Exemptions.Discover;
using Mignow.Atc.Application.Features.Exemptions.Get;
using Mignow.Atc.Application.Features.Exemptions.List;
using Mignow.Atc.Application.Features.Exemptions.ListRequests;
using Mignow.Atc.Application.Features.Exemptions.Send;
using Mignow.Atc.Application.Features.Exemptions.Simulate;
using Mignow.Atc.Application.Features.Exemptions.UpdateParams;

namespace Mignow.Atc.Api.Controllers;

/// <summary>Isenção ATC (skill atc-isencao): lote → descobrir → parâmetros → simular → enviar (confirmado) → conferir.</summary>
[Route("v1/exemption-batches")]
[Tags("Exemptions")]
public sealed class ExemptionsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult<ExemptionBatchResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExemptionBatchResponse>>>> List(
        [FromServices] IQueryHandler<ListExemptionBatchesQuery, Result<PagedResult<ExemptionBatchResponse>>> handler,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? sapSystemId = null, [FromQuery] Guid? sessionId = null,
        CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListExemptionBatchesQuery(page, pageSize, sapSystemId, sessionId), ct));

    [HttpPost]
    [ProducesResponseType<ApiResponse<ExemptionBatchCreatedResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExemptionBatchCreatedResponse>>> Create(CreateExemptionBatchCommand request,
        [FromServices] ICommandHandler<CreateExemptionBatchCommand, Result<ExemptionBatchCreatedResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(request, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ExemptionBatchResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExemptionBatchResponse>>> Get(Guid id,
        [FromServices] IQueryHandler<GetExemptionBatchQuery, Result<ExemptionBatchResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetExemptionBatchQuery(id), ct));

    [HttpGet("{id:guid}/requests")]
    [ProducesResponseType<ApiResponse<PagedResult<ExemptionRequestResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExemptionRequestResponse>>>> ListRequests(Guid id,
        [FromServices] IQueryHandler<ListExemptionRequestsQuery, Result<PagedResult<ExemptionRequestResponse>>> handler,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? status = null, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListExemptionRequestsQuery(id, page, pageSize, status), ct));

    [HttpPut("{id:guid}/params")]
    [ProducesResponseType<ApiResponse<ExemptionBatchResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExemptionBatchResponse>>> UpdateParams(Guid id, UpdateExemptionParamsRequest request,
        [FromServices] ICommandHandler<UpdateExemptionParamsCommand, Result<ExemptionBatchResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new UpdateExemptionParamsCommand(id, request.Approver, request.Reason, request.Validity, request.JustificationPrefix), ct));

    [HttpPost("{id:guid}/discover")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Discover(Guid id,
        [FromServices] ICommandHandler<StartExemptionDiscoverCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartExemptionDiscoverCommand(id), ct));

    [HttpPost("{id:guid}/simulate")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Simulate(Guid id,
        [FromServices] ICommandHandler<StartExemptionSimulateCommand, Result<RunResponse>> handler, [FromBody] ExemptionRunRequest? request = null,
        CancellationToken ct = default) =>
        AcceptedOrFailure(await handler.Handle(new StartExemptionSimulateCommand(id, request?.Seq), ct));

    /// <summary>GRAVA NO SAP (pedido ao aprovador). Só o que foi simulado e tem escopo; nunca em produção.</summary>
    [HttpPost("{id:guid}/send")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Send(Guid id,
        [FromServices] ICommandHandler<StartExemptionSendCommand, Result<RunResponse>> handler, [FromBody] ExemptionRunRequest? request = null,
        CancellationToken ct = default) =>
        AcceptedOrFailure(await handler.Handle(new StartExemptionSendCommand(id, request?.Seq), ct));

    [HttpPost("{id:guid}/check")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Check(Guid id,
        [FromServices] ICommandHandler<StartExemptionCheckCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartExemptionCheckCommand(id), ct));
}
