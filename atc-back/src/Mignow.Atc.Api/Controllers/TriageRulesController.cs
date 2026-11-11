using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.TriageRules;
using Mignow.Atc.Application.Features.TriageRules.Delete;
using Mignow.Atc.Application.Features.TriageRules.List;
using Mignow.Atc.Application.Features.TriageRules.Save;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Api.Controllers;

/// <summary>
/// Cadastro das regras de triagem. Padrão (R1..R9, produtos MIGNOW): lógica no código, edita descrição, escopo, ativa e objetos da R7/R9.
/// Novas: critérios (objetos referenciados, nome do objeto, texto do check) + efeito. O play aplica as ativas do tipo de projeto da sessão.
/// </summary>
[Route("v1/triage-rules")]
[Tags("TriageRules")]
public sealed class TriageRulesController : ApiControllerBase
{
    /// <summary>Regras com o escopo (todos, upgrade, conversão). <c>projectType</c> = só as ativas que valem para esse tipo de projeto.</summary>
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<TriageRuleDefinitionResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TriageRuleDefinitionResponse>>>> List(
        [FromServices] IQueryHandler<ListTriageRulesQuery, Result<IReadOnlyList<TriageRuleDefinitionResponse>>> handler,
        [FromQuery] ProjectType? projectType = null, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListTriageRulesQuery(projectType), ct));

    /// <summary>Nova regra (código livre, diferente das padrão).</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponse<TriageRuleDefinitionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<TriageRuleDefinitionResponse>>> Create(TriageRuleRequest r,
        [FromServices] ICommandHandler<SaveTriageRuleCommand, Result<TriageRuleDefinitionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(r.ToCommand(null), ct));

    /// <summary>Altera a regra de código <c>code</c> (<c>code</c> no corpo = novo código; padrão: o efeito fica no código).</summary>
    [HttpPut("{code}")]
    [ProducesResponseType<ApiResponse<TriageRuleDefinitionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriageRuleDefinitionResponse>>> Update(string code, TriageRuleRequest r,
        [FromServices] ICommandHandler<SaveTriageRuleCommand, Result<TriageRuleDefinitionResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(r.ToCommand(code), ct));

    /// <summary>Exclui uma regra nova (regra padrão: 409, desative no cadastro).</summary>
    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(string code, [FromServices] ICommandHandler<DeleteTriageRuleCommand, Result> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new DeleteTriageRuleCommand(code), ct));

    /// <summary>
    /// <c>code</c>: na criação, o código; na alteração, o novo código (vazio mantém). <c>effect</c>: Cancelado, Confirmado KB, Isenção ou signal
    /// (regra nova). <c>parameters</c>: parâmetros da lógica da regra padrão (nomes da própria regra, cada um com ao menos um valor).
    /// </summary>
    public sealed record TriageRuleRequest(string? Code, string Description, string Effect, TriageScope Scope, bool Active,
        IReadOnlyList<string>? RefObjects, string? ObjectPattern, string? MessagePattern, string? Obs,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? Parameters = null)
    {
        public SaveTriageRuleCommand ToCommand(string? currentCode) =>
            new(currentCode, Code, Description, Effect, Scope, Active, RefObjects, ObjectPattern, MessagePattern, Obs, Parameters);
    }
}
