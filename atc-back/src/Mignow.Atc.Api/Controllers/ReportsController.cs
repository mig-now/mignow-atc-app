using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.Reports.GetAnalysis;
using Mignow.Atc.Application.Features.Reports.GetData;
using Mignow.Atc.Application.Features.Reports.GetHtml;
using Mignow.Atc.Application.Features.Reports.Highlights;
using Mignow.Atc.Application.Features.Reports.History;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Domain.Report;

namespace Mignow.Atc.Api.Controllers;

[Route("v1")]
[Tags("Reports")]
public sealed class ReportsController : ApiControllerBase
{
    /// <summary>Dados do relatório final do cliente (= dados.json do coletar.py).</summary>
    [HttpGet("customers/{id:guid}/report")]
    [ProducesResponseType<ApiResponse<ReportData>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReportData>>> Get(Guid id, [FromServices] IQueryHandler<GetReportQuery, Result<ReportData>> handler,
        [FromQuery] Guid? sapSystemId = null, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetReportQuery(id, sapSystemId), ct));

    /// <summary>Relatório final em HTML (identidade MIGNOW); view = findings | objetos.</summary>
    [HttpGet("customers/{id:guid}/report.html")]
    [Produces("text/html")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetHtml(Guid id, [FromServices] IQueryHandler<GetReportHtmlQuery, Result<string>> handler,
        [FromQuery] string? view = null, [FromQuery] Guid? sapSystemId = null, CancellationToken ct = default) =>
        Html(await handler.Handle(new GetReportHtmlQuery(id, view, sapSystemId), ct));

    /// <summary>Resumo do relatório de análise: isencao | pendencias | mignow.</summary>
    [HttpGet("customers/{id:guid}/report/analyses/{kind}")]
    [ProducesResponseType<ApiResponse<AnalysisResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnalysisResponse>>> GetAnalysis(Guid id, string kind,
        [FromServices] IQueryHandler<GetAnalysisQuery, Result<AnalysisResponse>> handler, [FromQuery] Guid? sapSystemId = null, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetAnalysisQuery(id, kind, sapSystemId), ct));

    /// <summary>Página HTML do relatório de análise.</summary>
    [HttpGet("customers/{id:guid}/report/analyses/{kind}.html")]
    [Produces("text/html")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetAnalysisHtml(Guid id, string kind, [FromServices] IQueryHandler<GetAnalysisHtmlQuery, Result<string>> handler,
        [FromQuery] Guid? sapSystemId = null, CancellationToken ct = default) =>
        Html(await handler.Handle(new GetAnalysisHtmlQuery(id, kind, sapSystemId), ct));

    [HttpGet("customers/{id:guid}/report/highlights")]
    [ProducesResponseType<ApiResponse<HighlightsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HighlightsResponse>>> GetHighlights(Guid id,
        [FromServices] IQueryHandler<GetHighlightsQuery, Result<HighlightsResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetHighlightsQuery(id), ct));

    [HttpPut("customers/{id:guid}/report/highlights")]
    [ProducesResponseType<ApiResponse<HighlightsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HighlightsResponse>>> PutHighlights(Guid id, HighlightsRequest request,
        [FromServices] ICommandHandler<UpdateHighlightsCommand, Result<HighlightsResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new UpdateHighlightsCommand(id, request.Highlights ?? [], request.Pending ?? [], request.Premise), ct));

    /// <summary>Histórico de processo de um módulo (extraction | remediation | exemption | scan) por cliente e/ou ambiente.</summary>
    [HttpGet("history")]
    [ProducesResponseType<ApiResponse<IReadOnlyList<ProcessHistoryEntry>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProcessHistoryEntry>>>> GetHistory(
        [FromServices] IQueryHandler<GetHistoryQuery, Result<IReadOnlyList<ProcessHistoryEntry>>> handler,
        [FromQuery] Guid? customerId = null, [FromQuery] Guid? sapSystemId = null, [FromQuery] string module = "", CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetHistoryQuery(customerId, sapSystemId, module), ct));

    private ActionResult Html(Result<string> result) => result.IsSuccess ? Content(result.Value, "text/html; charset=utf-8") : HandleFailure(result.Error);

    public sealed record HighlightsRequest(IReadOnlyList<HighlightText>? Highlights, IReadOnlyList<PendingText>? Pending, ManualPremise? Premise);
}
