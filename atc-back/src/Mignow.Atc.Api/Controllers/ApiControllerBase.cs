using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Api.Controllers;

/// <summary>
/// Controller fino: Error.Type → status HTTP; <c>detail</c> = código estável (o front lê como <c>code</c>);
/// <see cref="Error.Data"/> vai em <c>data</c> (ex.: o run que bloqueia o ambiente no 409).
/// </summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult HandleFailure(Error error)
    {
        var result = Problem(
            title: error.Message,
            statusCode: error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Upstream => StatusCodes.Status502BadGateway,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest,
            },
            detail: error.Code);
        if (error.Data is not null && result is ObjectResult { Value: ProblemDetails problem })
        {
            problem.Extensions["data"] = error.Data;
            // 409 de ambiente ocupado: o run que bloqueia, direto no ProblemDetails (o front lê sem abrir o data)
            if (error.Data is RunResponse blocking)
            {
                problem.Extensions["blockingRunId"] = blocking.Id;
                problem.Extensions["blockingRunKind"] = blocking.Kind.ToString();
            }
        }
        return result;
    }

    protected ActionResult<ApiResponse<T>> OkOrFailure<T>(Result<T> result) =>
        result.IsSuccess ? Ok(ApiResponse<T>.Ok(result.Value)) : HandleFailure(result.Error);

    protected ActionResult OkOrFailure(Result result) => result.IsSuccess ? NoContent() : HandleFailure(result.Error);

    /// <summary>Ações que disparam execução: 202 com o run.</summary>
    protected ActionResult<ApiResponse<T>> AcceptedOrFailure<T>(Result<T> result) =>
        result.IsSuccess ? Accepted(ApiResponse<T>.Ok(result.Value)) : HandleFailure(result.Error);
}
