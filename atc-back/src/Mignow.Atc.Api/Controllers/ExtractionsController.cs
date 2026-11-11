using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Features.Extractions.Check;
using Mignow.Atc.Application.Features.Extractions.Get;
using Mignow.Atc.Application.Features.Extractions.GetFile;
using Mignow.Atc.Application.Features.Extractions.Latest;
using Mignow.Atc.Application.Features.Extractions.List;
using Mignow.Atc.Application.Features.Extractions.ListFindings;
using Mignow.Atc.Application.Features.Extractions.Monitor;
using Mignow.Atc.Application.Features.Extractions.Start;
using Mignow.Atc.Application.Modules.Extraction;

namespace Mignow.Atc.Api.Controllers;

/// <summary>Extração dos findings da última execução concluída do ATC (série ZMIG_/Z_MIG_) e os CSVs da skill atc-extracao.</summary>
[Route("v1")]
[Tags("Extractions")]
public sealed class ExtractionsController : ApiControllerBase
{
    /// <summary>Verifica no SAP a última execução concluída do ATC: série, display id, data, totais e se já foi extraída.</summary>
    [HttpGet("sap-systems/{id:guid}/atc/latest")]
    [ProducesResponseType<ApiResponse<AtcLatestResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ApiResponse<AtcLatestResponse>>> Latest(Guid id,
        [FromServices] IQueryHandler<GetLatestAtcQuery, Result<AtcLatestResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetLatestAtcQuery(id), ct));

    /// <summary>A mesma verificação como execução (Run ExtractionCheck): o resultado fica no run.</summary>
    [HttpPost("sap-systems/{id:guid}/atc/check")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Check(Guid id,
        [FromServices] ICommandHandler<StartExtractionCheckCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartExtractionCheckCommand(id), ct));

    [HttpGet("extractions")]
    [ProducesResponseType<ApiResponse<PagedResult<ExtractionResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExtractionResponse>>>> List(
        [FromServices] IQueryHandler<ListExtractionsQuery, Result<PagedResult<ExtractionResponse>>> handler,
        [FromQuery] Guid? sapSystemId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListExtractionsQuery(sapSystemId, page, pageSize), ct));

    /// <summary>Monitor: ambientes ativos (de um cliente, opcional) com a última execução de extração e a última extração gravada.</summary>
    [HttpGet("extractions/monitor")]
    [ProducesResponseType<ApiResponse<IReadOnlyList<ExtractionMonitorRow>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExtractionMonitorRow>>>> Monitor(
        [FromServices] IQueryHandler<GetExtractionMonitorQuery, Result<IReadOnlyList<ExtractionMonitorRow>>> handler,
        [FromQuery] Guid? customerId = null, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetExtractionMonitorQuery(customerId), ct));

    /// <summary>Extrai a última execução concluída (Run Extraction). Já extraída: nada a fazer, salvo <c>force</c>.</summary>
    [HttpPost("extractions")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Start(StartExtractionCommand request,
        [FromServices] ICommandHandler<StartExtractionCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(request, ct));

    [HttpGet("extractions/{id:guid}")]
    [ProducesResponseType<ApiResponse<ExtractionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExtractionResponse>>> Get(Guid id,
        [FromServices] IQueryHandler<GetExtractionQuery, Result<ExtractionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetExtractionQuery(id), ct));

    /// <summary>Findings da extração (paginado). Filtros: prioridade, objeto (contém), baseline; <c>includeColumns</c> traz todas as colunas.</summary>
    [HttpGet("extractions/{id:guid}/findings")]
    [ProducesResponseType<ApiResponse<PagedResult<FindingResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<FindingResponse>>>> Findings(Guid id,
        [FromServices] IQueryHandler<ListFindingsQuery, Result<PagedResult<FindingResponse>>> handler,
        [FromQuery] int? priority = null, [FromQuery(Name = "object")] string? objectName = null, [FromQuery] bool? inBaseline = null,
        [FromQuery] bool includeColumns = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListFindingsQuery(id, priority, objectName, inBaseline, includeColumns, page, pageSize), ct));

    /// <summary>
    /// CSV (UTF-8 com BOM, <c>;</c>): <c>total</c>, <c>novos</c> (sem baseline) ou <c>remediacao</c> (layout da atc-remediacao, só os novos).
    /// <c>priorities=1,2</c> restringe as linhas.
    /// </summary>
    [HttpGet("extractions/{id:guid}/files/{kind:regex(^(total|novos|remediacao)$)}")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> File(Guid id, string kind,
        [FromServices] IQueryHandler<GetExtractionFileQuery, Result<ExtractionFileResponse>> handler, [FromQuery] string? priorities = null,
        CancellationToken ct = default)
    {
        var fileKind = kind switch
        {
            "novos" => ExtractionFileKind.New,
            "remediacao" => ExtractionFileKind.Remediation,
            _ => ExtractionFileKind.Total,
        };
        var result = await handler.Handle(new GetExtractionFileQuery(id, fileKind, priorities), ct);
        return result.IsSuccess ? base.File(result.Value.Content, "text/csv; charset=utf-8", result.Value.FileName) : HandleFailure(result.Error);
    }
}
