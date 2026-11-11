using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Features.Sessions;
using Mignow.Atc.Application.Features.Sessions.Create;
using Mignow.Atc.Application.Features.Sessions.Decide;
using Mignow.Atc.Application.Features.Sessions.Export;
using Mignow.Atc.Application.Features.Sessions.Get;
using Mignow.Atc.Application.Features.Sessions.GetItem;
using Mignow.Atc.Application.Features.Sessions.Groups;
using Mignow.Atc.Application.Features.Sessions.History;
using Mignow.Atc.Application.Features.Sessions.ImportCsv;
using Mignow.Atc.Application.Features.Sessions.List;
using Mignow.Atc.Application.Features.Sessions.ListItems;
using Mignow.Atc.Application.Features.Sessions.Monitor;
using Mignow.Atc.Application.Features.Sessions.Reuse;
using Mignow.Atc.Application.Features.Sessions.StartConfirmR1;
using Mignow.Atc.Application.Features.Sessions.StartAutoRemediation;
using Mignow.Atc.Application.Features.Sessions.Sources;
using Mignow.Atc.Application.Features.Sessions.StartReadSources;
using Mignow.Atc.Application.Features.Sessions.SetAutoMode;
using Mignow.Atc.Application.Features.Sessions.SetRequest;
using Mignow.Atc.Application.Features.Sessions.Triage;
using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Api.Controllers;

/// <summary>Sessões de remediação: importação, leitura de fontes, triagem, grupos, decisões, exportação e histórico (a proposta e a aplicação ficam em controller próprio).</summary>
[Route("v1/sessions")]
[Tags("Sessions")]
public sealed class SessionsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult<SessionResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SessionResponse>>>> List(
        [FromServices] IQueryHandler<ListSessionsQuery, Result<PagedResult<SessionResponse>>> handler,
        [FromQuery] Guid? sapSystemId = null, [FromQuery] Guid? customerId = null, [FromQuery] bool? archived = null, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListSessionsQuery(sapSystemId, customerId, archived, page, pageSize), ct));

    /// <summary>Cria a sessão a partir de uma extração (findings sem baseline) e das prioridades. Reimportar arquiva o ciclo anterior.</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponse<SessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Create(CreateSessionCommand request,
        [FromServices] ICommandHandler<CreateSessionCommand, Result<SessionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(request, ct));

    /// <summary>Monitor: sessões (cliente opcional; arquivadas só com includeArchived) com a situação dos itens e a última execução.</summary>
    [HttpGet("monitor")]
    [ProducesResponseType<ApiResponse<IReadOnlyList<RemediationMonitorRow>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RemediationMonitorRow>>>> Monitor(
        [FromServices] IQueryHandler<GetRemediationMonitorQuery, Result<IReadOnlyList<RemediationMonitorRow>>> handler,
        [FromQuery] Guid? customerId = null, [FromQuery] bool includeArchived = false, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetRemediationMonitorQuery(customerId, includeArchived), ct));

    /// <summary>Cria a sessão a partir de um CSV enviado (multipart: file, sapSystemId, priorities, request).</summary>
    [HttpPost("import-csv")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ApiResponse<SessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> ImportCsv([FromForm] ImportCsvForm form,
        [FromServices] ICommandHandler<ImportCsvSessionCommand, Result<SessionResponse>> handler, CancellationToken ct)
    {
        if (form.File is null || form.File.Length == 0)
            return HandleFailure(Error.Validation("Session.CsvEmpty", "CSV vazio."));
        await using var stream = form.File.OpenReadStream();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return OkOrFailure(await handler.Handle(new ImportCsvSessionCommand(form.SapSystemId, form.File.FileName, buffer.ToArray(), form.Priorities, form.Request), ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<SessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Get(Guid id,
        [FromServices] IQueryHandler<GetSessionQuery, Result<SessionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetSessionQuery(id), ct));

    /// <summary>Itens. Filtros: status (pending|confirmedKb|confirmedAi|cancelled|exemption), priority, objectName, group (objeto referenciado), triage (regra).</summary>
    [HttpGet("{id:guid}/items")]
    [ProducesResponseType<ApiResponse<PagedResult<ItemResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<ItemResponse>>>> Items(Guid id,
        [FromServices] IQueryHandler<ListSessionItemsQuery, Result<PagedResult<ItemResponse>>> handler,
        [FromQuery] string? status = null, [FromQuery] string? priority = null, [FromQuery] string? objectName = null, [FromQuery] string? group = null,
        [FromQuery] string? triage = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListSessionItemsQuery(id, status, priority, objectName, group, triage, page, pageSize), ct));

    [HttpGet("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType<ApiResponse<ItemDetailResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDetailResponse>>> GetItem(Guid id, Guid itemId,
        [FromServices] IQueryHandler<GetSessionItemQuery, Result<ItemDetailResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetSessionItemQuery(id, itemId), ct));

    /// <summary>Liga/desliga o modo automático da sessão (exigido pelo AutoConfirm do Propose).</summary>
    [HttpPut("{id:guid}/auto-mode")]
    [ProducesResponseType<ApiResponse<SessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> SetAutoMode(Guid id, AutoModeRequest request,
        [FromServices] ICommandHandler<SetAutoModeCommand, Result<SessionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new SetAutoModeCommand(id, request.Enabled), ct));

    public sealed record AutoModeRequest(bool Enabled);

    /// <summary>Define a request de transporte da sessão (exigida para aplicar, inclusive pelo play).</summary>
    [HttpPut("{id:guid}/request")]
    [ProducesResponseType<ApiResponse<SessionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> SetRequest(Guid id, SessionRequestBody request,
        [FromServices] ICommandHandler<SetSessionRequestCommand, Result<SessionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new SetSessionRequestCommand(id, request.Request), ct));

    public sealed record SessionRequestBody(string Request);

    /// <summary>
    /// "Play" da remediação (202, run AutoRemediation): itens escolhidos (vazio = todos os pendentes) passam por fontes, triagem, R1, proposta
    /// KB/IA, confirmação, simulação, revisão e aplicação, sem parada; o que não passa nas travas fica pendente para o consultor.
    /// </summary>
    [HttpPost("{id:guid}/auto-remediation")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> StartAutoRemediation(Guid id, AutoRemediationRequest request,
        [FromServices] ICommandHandler<StartAutoRemediationCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartAutoRemediationCommand(id, request.ItemIds), ct));

    /// <summary><c>itemIds</c>: itens escolhidos (null = todos os pendentes). As regras de triagem vêm do tipo de projeto da sessão.</summary>
    public sealed record AutoRemediationRequest(IReadOnlyList<Guid>? ItemIds);

    /// <summary>Run ReadSources (202): lê os fontes do lote, em paralelo, com cache; standard só com includeStandard.</summary>
    [HttpPost("{id:guid}/read-sources")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> ReadSources(Guid id, ReadSourcesRequest request,
        [FromServices] ICommandHandler<StartReadSourcesCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartReadSourcesCommand(id, request.Batch, request.IncludeStandard, request.Force), ct));

    /// <summary>Prévia da triagem R1..R9 (+ produtos MIGNOW): contagens por regra e itens afetados, sem gravar.</summary>
    [HttpGet("{id:guid}/triage")]
    [ProducesResponseType<ApiResponse<TriageResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriageResponse>>> Triage(Guid id,
        [FromServices] IQueryHandler<TriagePreviewQuery, Result<TriageResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new TriagePreviewQuery(id), ct));

    [HttpPost("{id:guid}/triage/apply")]
    [ProducesResponseType<ApiResponse<TriageResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TriageResponse>>> TriageApply(Guid id, TriageApplyRequest request,
        [FromServices] ICommandHandler<TriageApplyCommand, Result<TriageResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new TriageApplyCommand(id, request.Rules), ct));

    /// <summary>Run ConfirmR1 (202): confere a R1 no DDIC (Isenção ou volta a pendente). Obrigatório depois de aplicar a R1.</summary>
    [HttpPost("{id:guid}/confirm-r1")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> ConfirmR1(Guid id, ConfirmR1Request request,
        [FromServices] ICommandHandler<StartConfirmR1Command, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartConfirmR1Command(id, request.ItemNs, request.Apply), ct));

    /// <summary>Grupos do lote (objeto referenciado + forma de chamada): solução da KB, complexidade e modelo sugerido.</summary>
    [HttpGet("{id:guid}/groups")]
    [ProducesResponseType<ApiResponse<GroupingResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GroupingResult>>> Groups(Guid id,
        [FromServices] IQueryHandler<GetGroupsQuery, Result<GroupingResult>> handler, [FromQuery] int? batch = null, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetGroupsQuery(id, batch), ct));

    /// <summary>Decisão do consultor: Cancel | Exemption | ConfirmKb | ConfirmIa | Reset | AcceptSuggestion (a sugestão da proposta de cada item).</summary>
    [HttpPost("{id:guid}/items/decision")]
    [ProducesResponseType<ApiResponse<DecideItemsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<DecideItemsResponse>>> Decide(Guid id, DecisionRequest request,
        [FromServices] ICommandHandler<DecideItemsCommand, Result<DecideItemsResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new DecideItemsCommand(id, request.ItemIds, request.Decision, request.Obs, request.Replaces, request.Reason), ct));

    /// <summary>atc_status.csv: o CSV original com a coluna "Status Remediação" (mesma codificação e delimitador).</summary>
    [HttpGet("{id:guid}/export")]
    [Produces("text/csv")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Export(Guid id, [FromServices] IQueryHandler<ExportSessionQuery, Result<ExportedCsv>> handler, CancellationToken ct)
    {
        var result = await handler.Handle(new ExportSessionQuery(id), ct);
        return result.IsFailure ? HandleFailure(result.Error) : File(result.Value.Content, "text/csv", result.Value.FileName);
    }

    /// <summary>Reaproveita fichas confirmadas de outra sessão do mesmo ambiente (só avalia; não altera status).</summary>
    [HttpPost("{id:guid}/reuse")]
    [ProducesResponseType<ApiResponse<ReuseResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReuseResult>>> Reuse(Guid id, ReuseRequest request,
        [FromServices] IQueryHandler<ReuseSessionQuery, Result<ReuseResult>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new ReuseSessionQuery(id, request.FromSessionId, request.ItemNs), ct));

    /// <summary>Objetos da sessão com fonte lido (a leitura é automática: ao carregar os itens e no play).</summary>
    [HttpGet("{id:guid}/sources")]
    [ProducesResponseType<ApiResponse<SessionSourcesResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SessionSourcesResponse>>> Sources(Guid id,
        [FromServices] IQueryHandler<ListSessionSourcesQuery, Result<SessionSourcesResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new ListSessionSourcesQuery(id), ct));

    /// <summary>Conteúdo de um fonte lido de objeto da sessão (somente leitura).</summary>
    [HttpGet("{id:guid}/sources/{sourceId:guid}")]
    [ProducesResponseType<ApiResponse<SessionSourceContent>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SessionSourceContent>>> Source(Guid id, Guid sourceId,
        [FromServices] IQueryHandler<GetSessionSourceQuery, Result<SessionSourceContent>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetSessionSourceQuery(id, sourceId), ct));

    /// <summary>Antes/depois por situação, transições, linha do tempo, tempo líquido e Runs com tokens.</summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType<ApiResponse<SessionHistory>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SessionHistory>>> History(Guid id,
        [FromServices] IQueryHandler<GetSessionHistoryQuery, Result<SessionHistory>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetSessionHistoryQuery(id), ct));
}
