using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Features.AiProviders.Create;
using Mignow.Atc.Application.Features.AiProviders.Delete;
using Mignow.Atc.Application.Features.AiProviders.List;
using Mignow.Atc.Application.Features.AiProviders.Update;

namespace Mignow.Atc.Api.Controllers;

[Route("v1/ai-providers")]
[Tags("AiProviders")]
public sealed class AiProvidersController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<AiProviderResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AiProviderResponse>>>> List(
        [FromServices] IQueryHandler<ListAiProvidersQuery, Result<IReadOnlyList<AiProviderResponse>>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new ListAiProvidersQuery(), ct));

    [HttpPost]
    [ProducesResponseType<ApiResponse<AiProviderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AiProviderResponse>>> Create(CreateAiProviderCommand request,
        [FromServices] ICommandHandler<CreateAiProviderCommand, Result<AiProviderResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(request, ct));

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<AiProviderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AiProviderResponse>>> Update(Guid id, UpdateAiProviderRequest r,
        [FromServices] ICommandHandler<UpdateAiProviderCommand, Result<AiProviderResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new UpdateAiProviderCommand(id, r.Name, r.Kind, r.Model, r.ApiKey, r.IsDefault, r.Active), ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(Guid id, [FromServices] ICommandHandler<DeleteAiProviderCommand, Result> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new DeleteAiProviderCommand(id), ct));
}
