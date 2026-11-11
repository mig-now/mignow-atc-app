using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Features.Scans.Create;
using Mignow.Atc.Application.Features.Scans.Get;
using Mignow.Atc.Application.Features.Scans.GetP1File;
using Mignow.Atc.Application.Features.Scans.GetSummaryFile;
using Mignow.Atc.Application.Features.Scans.Import;
using Mignow.Atc.Application.Features.Scans.List;
using Mignow.Atc.Application.Features.Scans.ListOccurrences;
using Mignow.Atc.Application.Features.Scans.Rules;
using Mignow.Atc.Application.Features.Scans.SetScope;
using Mignow.Atc.Application.Features.Scans.StartCleanup;
using Mignow.Atc.Application.Features.Scans.StartExecute;
using Mignow.Atc.Application.Features.Scans.StartInventory;
using Mignow.Atc.Application.Features.Scans.StartSimulate;
using Mignow.Atc.Application.Features.Sessions;

namespace Mignow.Atc.Api.Controllers;

/// <summary>
/// Varredura MIGNOW (skill atc-extracao-mignow): regex complementar ao ATC por classes temporárias em <c>$TMP</c> (criadas, executadas e
/// apagadas), cruzamento com a extração do ATC e CSV P1. Só extrai: nunca altera os objetos encontrados.
/// </summary>
[Route("v1/scans")]
[Tags("Scans")]
public sealed class ScansController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult<ScanResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ScanResponse>>>> List(
        [FromServices] IQueryHandler<ListScansQuery, Result<PagedResult<ScanResponse>>> handler,
        [FromQuery] Guid? sapSystemId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListScansQuery(sapSystemId, page, pageSize), ct));

    /// <summary>Cria a varredura (extração = a mais recente do ambiente se não informada) e enfileira o inventário.</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponse<ScanStartedResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ScanStartedResponse>>> Create(CreateScanCommand request,
        [FromServices] ICommandHandler<CreateScanCommand, Result<ScanStartedResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(request, ct));

    /// <summary>Regras (<c>regras.json</c> do caminho configurado em Settings), somente leitura.</summary>
    [HttpGet("rules")]
    [ProducesResponseType<ApiResponse<ScanRulesResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ScanRulesResponse>>> Rules(
        [FromServices] IQueryHandler<ListScanRulesQuery, Result<ScanRulesResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new ListScanRulesQuery(), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ScanResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ScanResponse>>> Get(Guid id,
        [FromServices] IQueryHandler<GetScanQuery, Result<ScanResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetScanQuery(id), ct));

    /// <summary>Refaz o inventário do escopo Z/Y (descarta lotes, ocorrências e resumo).</summary>
    [HttpPost("{id:guid}/inventory")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Inventory(Guid id, [FromQuery] bool? includeGenerated,
        [FromServices] ICommandHandler<StartInventoryCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartInventoryCommand(id, includeGenerated), ct));

    public sealed record ScopeRequest(IReadOnlyList<string> RuleIds);

    [HttpPut("{id:guid}/scope")]
    [ProducesResponseType<ApiResponse<ScanResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ScanResponse>>> SetScope(Guid id, ScopeRequest request,
        [FromServices] ICommandHandler<SetScopeCommand, Result<ScanResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new SetScopeCommand(id, request.RuleIds), ct));

    /// <summary>Simulação (somente leitura): papel do mandante (T000), nomes das classes livres e validação no ADT.</summary>
    [HttpPost("{id:guid}/simulate")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Simulate(Guid id, [FromQuery] string? lots, [FromQuery] int? parallel,
        [FromServices] ICommandHandler<StartSimulateCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartSimulateCommand(id, lots, parallel), ct));

    /// <summary>Executa (cria/roda/apaga as classes em $TMP; liberado sem nova confirmação; nunca em produção): lote 1, depois o resto em paralelo, e cruza com o ATC.</summary>
    [HttpPost("{id:guid}/execute")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Execute(Guid id, StartExecuteBody? request,
        [FromServices] ICommandHandler<StartExecuteCommand, Result<RunResponse>> handler, CancellationToken ct)
    {
        request ??= new StartExecuteBody();
        return AcceptedOrFailure(await handler.Handle(
            new StartExecuteCommand(id, request.Lots, request.Redo, request.Parallel, request.IncludeOtherLine, request.CompareOriginal), ct));
    }

    public sealed record StartExecuteBody(string? Lots = null, bool Redo = false, int? Parallel = null, bool IncludeOtherLine = false, bool CompareOriginal = true);

    /// <summary>Remove resíduos: apaga a classe base e as <c>_1.._9</c> que tenham ficado em $TMP.</summary>
    [HttpPost("{id:guid}/cleanup")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Cleanup(Guid id,
        [FromServices] ICommandHandler<StartCleanupCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartCleanupCommand(id), ct));

    [HttpGet("{id:guid}/occurrences")]
    [ProducesResponseType<ApiResponse<PagedResult<ScanOccurrenceResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<ScanOccurrenceResponse>>>> Occurrences(Guid id,
        [FromServices] IQueryHandler<ListOccurrencesQuery, Result<PagedResult<ScanOccurrenceResponse>>> handler,
        [FromQuery] string? ruleId = null, [FromQuery] string? cross = null, [FromQuery] string? status = null, [FromQuery] string? objectName = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListOccurrencesQuery(id, ruleId, cross, status, objectName, page, pageSize), ct));

    /// <summary>CSV P1 no layout da atc-remediacao + <c>Origem;Regra;Fonte ADT;Linha;Linha fim;Cruzamento;Trecho</c>.</summary>
    [HttpGet("{id:guid}/files/p1")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> P1(Guid id, [FromQuery] bool? includeOtherLine,
        [FromServices] IQueryHandler<GetP1FileQuery, Result<ScanFileResponse>> handler, CancellationToken ct)
    {
        var result = await handler.Handle(new GetP1FileQuery(id, includeOtherLine), ct);
        return result.IsFailure ? HandleFailure(result.Error) : File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>Resumo do cruzamento em markdown (<c>resumo.md</c>).</summary>
    [HttpGet("{id:guid}/files/summary")]
    [Produces("text/markdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Summary(Guid id,
        [FromServices] IQueryHandler<GetSummaryFileQuery, Result<ScanFileResponse>> handler, CancellationToken ct)
    {
        var result = await handler.Handle(new GetSummaryFileQuery(id), ct);
        return result.IsFailure ? HandleFailure(result.Error) : File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>Importa o CSV P1 na remediação (sessão Origin = MIGNOW). Só a pedido do consultor; arquiva a sessão anterior do ambiente.</summary>
    [HttpPost("{id:guid}/import")]
    [ProducesResponseType<ApiResponse<SessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Import(Guid id, [FromQuery] bool? includeOtherLine,
        [FromServices] ICommandHandler<ImportScanCommand, Result<SessionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new ImportScanCommand(id, includeOtherLine), ct));
}
