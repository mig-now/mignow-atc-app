using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Api.Controllers;

[Route("v1/health")]
[Tags("Health")]
[AllowAnonymous]
public sealed class HealthController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<HealthResponse>>(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<HealthResponse>> Get() => Ok(ApiResponse<HealthResponse>.Ok(new HealthResponse("ok")));
}
