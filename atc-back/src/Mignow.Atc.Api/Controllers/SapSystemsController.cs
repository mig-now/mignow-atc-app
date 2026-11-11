using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Features.SapSystems.ChangePassword;
using Mignow.Atc.Application.Features.SapSystems.Create;
using Mignow.Atc.Application.Features.SapSystems.Delete;
using Mignow.Atc.Application.Features.SapSystems.Get;
using Mignow.Atc.Application.Features.SapSystems.List;
using Mignow.Atc.Application.Features.SapSystems.StartConnectionTest;
using Mignow.Atc.Application.Features.SapSystems.TestConnection;
using Mignow.Atc.Application.Features.SapSystems.Update;

namespace Mignow.Atc.Api.Controllers;

[Route("v1/sap-systems")]
[Tags("SapSystems")]
public sealed class SapSystemsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult<SapSystemResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SapSystemResponse>>>> List(
        [FromServices] IQueryHandler<ListSapSystemsQuery, Result<PagedResult<SapSystemResponse>>> handler,
        [FromQuery] Guid? customerId = null, [FromQuery] bool? active = null, [FromQuery] string? search = null, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListSapSystemsQuery(customerId, active, search, page, pageSize), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<SapSystemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SapSystemResponse>>> Get(Guid id,
        [FromServices] IQueryHandler<GetSapSystemQuery, Result<SapSystemResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetSapSystemQuery(id), ct));

    [HttpPost]
    [ProducesResponseType<ApiResponse<SapSystemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<SapSystemResponse>>> Create(CreateSapSystemCommand request,
        [FromServices] ICommandHandler<CreateSapSystemCommand, Result<SapSystemResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(request, ct));

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<SapSystemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<SapSystemResponse>>> Update(Guid id, UpdateSapSystemRequest r,
        [FromServices] ICommandHandler<UpdateSapSystemCommand, Result<SapSystemResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new UpdateSapSystemCommand(id, r.Sid, r.Client, r.Host, r.AdtPort, r.Https, r.Language, r.CertThumbprint,
            r.IsProduction, r.ProjectType, r.Author, r.DefaultRequest, r.ExemptionApprover, r.Username, r.Active), ct));

    /// <summary>Troca a senha (vai para o cofre; nunca volta pela API).</summary>
    [HttpPut("{id:guid}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ChangePassword(Guid id, ChangePasswordRequest r,
        [FromServices] ICommandHandler<ChangeSapSystemPasswordCommand, Result> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new ChangeSapSystemPasswordCommand(id, r.Password), ct));

    /// <summary>Teste de conexão síncrono (discovery, release, T000); fixa o certificado no primeiro teste.</summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType<ApiResponse<ConnectionTestResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ConnectionTestResponse>>> Test(Guid id,
        [FromServices] ICommandHandler<TestSapConnectionCommand, Result<ConnectionTestResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new TestSapConnectionCommand(id), ct));

    /// <summary>Teste de conexão como execução (Run ConnectionTest): 202 com o run; 409 com o run que bloqueia o ambiente.</summary>
    [HttpPost("{id:guid}/test/run")]
    [ProducesResponseType<ApiResponse<RunResponse>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RunResponse>>> TestRun(Guid id,
        [FromServices] ICommandHandler<StartConnectionTestCommand, Result<RunResponse>> handler, CancellationToken ct) =>
        AcceptedOrFailure(await handler.Handle(new StartConnectionTestCommand(id), ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(Guid id, [FromServices] ICommandHandler<DeleteSapSystemCommand, Result> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new DeleteSapSystemCommand(id), ct));
}
