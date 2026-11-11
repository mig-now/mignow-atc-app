using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.Knowledge.GetElement;
using Mignow.Atc.Application.Features.Knowledge.Index;
using Mignow.Atc.Application.Features.Knowledge.Lookup;
using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Api.Controllers;

/// <summary>Consulta à KB markdown (KB_ATC). A escrita (contribuições/fatos) vem só pelas telas de remediação.</summary>
[Route("v1/kb")]
[Tags("Knowledge")]
public sealed class KnowledgeController : ApiControllerBase
{
    /// <summary>Índice por elemento (filtros opcionais: categoria, texto) e por padrão.</summary>
    [HttpGet("index")]
    [ProducesResponseType<ApiResponse<KbIndexResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<KbIndexResponse>>> Index(
        [FromServices] IQueryHandler<GetKbIndexQuery, Result<KbIndexResponse>> handler,
        [FromQuery] string? category = null, [FromQuery] string? search = null, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetKbIndexQuery(category, search), ct));

    /// <summary>Elemento completo: cartões da base, contribuições, fatos e substituições. <c>element</c> = slug do índice.</summary>
    [HttpGet("elements/{category}/{element}")]
    [ProducesResponseType<ApiResponse<KbElementDetail>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<KbElementDetail>>> GetElement(string category, string element,
        [FromServices] IQueryHandler<GetKbElementQuery, Result<KbElementDetail>> handler, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new GetKbElementQuery(category, element), ct));

    /// <summary>Equivale ao <c>kb_lookup</c> do atc.py: melhores soluções para o objeto referenciado.</summary>
    [HttpGet("lookup")]
    [ProducesResponseType<ApiResponse<KbLookupResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<KbLookupResult>>> Lookup(
        [FromServices] IQueryHandler<KbLookupQuery, Result<KbLookupResult>> handler,
        [FromQuery(Name = "ref")] string reference, [FromQuery] string? robt = null, [FromQuery] int top = 3, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new KbLookupQuery(reference, robt, top), ct));
}
