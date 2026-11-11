using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Features.Me.Get;

namespace Mignow.Atc.Api.Controllers;

[Route("v1/me")]
[Tags("Me")]
public sealed class MeController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<MeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MeResponse>>> Get([FromServices] IQueryHandler<GetMeQuery, Result<MeResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetMeQuery(), ct));
}
