using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Features.Settings.Get;
using Mignow.Atc.Application.Features.Settings.Update;

namespace Mignow.Atc.Api.Controllers;

[Route("v1/settings")]
[Tags("Settings")]
public sealed class SettingsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<SettingsResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SettingsResponse>>> Get(
        [FromServices] IQueryHandler<GetSettingsQuery, Result<SettingsResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new GetSettingsQuery(), ct));

    [HttpPut]
    [ProducesResponseType<ApiResponse<SettingsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SettingsResponse>>> Update(UpdateSettingsCommand request,
        [FromServices] ICommandHandler<UpdateSettingsCommand, Result<SettingsResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(request, ct));
}
