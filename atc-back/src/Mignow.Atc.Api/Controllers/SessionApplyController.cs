using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Features.SessionApply.ActivationDecision;
using Mignow.Atc.Application.Features.SessionApply.Apply;
using Mignow.Atc.Application.Features.SessionApply.ApplySimulate;
using Mignow.Atc.Application.Features.SessionApply.Decide;
using Mignow.Atc.Application.Features.SessionApply.GetProposal;
using Mignow.Atc.Application.Features.SessionApply.Propose;

namespace Mignow.Atc.Api.Controllers;

/// <summary>Remediação – proposta e aplicação no SAP (passos 5 a 9 do fluxo da sessão).</summary>
[Route("v1/sessions/{id:guid}")]
[Tags("SessionApply")]
public sealed class SessionApplyController : ApiControllerBase
{
    /// <summary>Run Propose (KB ou Análise da IA): 202 com o run; 409 se o ambiente já tem execução ativa.</summary>
    [HttpPost("propose")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Propose(Guid id, SessionApplyProposeRequest r,
        [FromServices] ICommandHandler<ProposeCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new ProposeCommand(id, r.ItemIds, r.GroupKeys, r.Mode, r.AutoConfirm), ct));

    /// <summary>Decisão do consultor: ConfirmKb | ConfirmIa (grava a contribuição e os fatos na KB).</summary>
    [HttpPost("proposals/decision")]
    [ProducesResponseType<ApiResponse<DecideProposalResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<DecideProposalResponse>>> Decide(Guid id, SessionApplyDecisionRequest r,
        [FromServices] ICommandHandler<DecideProposalCommand, Result<DecideProposalResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new DecideProposalCommand(id, r.ItemIds, r.Decision, r.Obs, r.Replaces, r.Reason, r.Facts), ct));

    /// <summary>Run ApplySimulate: simulação (não grava no SAP) + revisão por IA.</summary>
    [HttpPost("apply-simulate")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> ApplySimulate(Guid id, SessionApplyItemsRequest r,
        [FromServices] ICommandHandler<ApplySimulateCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new ApplySimulateCommand(id, r.ItemIds, r.AllowTableStructure, r.DdicCreate), ct));

    /// <summary>Run Apply: grava no SAP, objeto por objeto, em fila (nunca em produção).</summary>
    [HttpPost("apply")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> Apply(Guid id, SessionApplyItemsRequest r,
        [FromServices] ICommandHandler<ApplyCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new ApplyCommand(id, r.ItemIds, r.AllowTableStructure, r.DdicCreate), ct));

    /// <summary>Erro de ativação: keepInactive (200) ou rollback (202 com o run Rollback).</summary>
    [HttpPost("items/{itemId:guid}/activation-decision")]
    [ProducesResponseType<ApiResponse<ActivationDecisionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ActivationDecisionResponse>>> ActivationDecision(Guid id, Guid itemId, SessionApplyActivationRequest r,
        [FromServices] ICommandHandler<ActivationDecisionCommand, Result<ActivationDecisionResponse>> handler, CancellationToken ct)
    {
        var result = await handler.Handle(new ActivationDecisionCommand(id, itemId, r.Decision), ct);
        return result.IsSuccess && result.Value.Run is not null ? Accepted(ApiResponse<ActivationDecisionResponse>.Ok(result.Value)) : OkOrFailure(result);
    }

    /// <summary>Proposta do item: blocos, diff, revisão por IA, alertas, sintaxe, preparação e aplicação.</summary>
    [HttpGet("items/{itemId:guid}/proposal")]
    [ProducesResponseType<ApiResponse<ProposalResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProposalResponse>>> GetProposal(Guid id, Guid itemId,
        [FromServices] IQueryHandler<GetProposalQuery, Result<ProposalResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetProposalQuery(id, itemId), ct));
}
