using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Mignow.Atc.Application.Exceptions;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Api.Middleware;

/// <summary>Exceções → ProblemDetails (application/problem+json), título localizado. Nunca loga token nem segredo.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IStringLocalizer<Errors> l)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.GroupBy(e => ToCamel(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await Write(context, StatusCodes.Status400BadRequest, new ValidationProblemDetails(errors)
            {
                Title = l.Text("Validation.Failed"), Detail = "Validation.Failed", Status = StatusCodes.Status400BadRequest,
            });
        }
        catch (DomainException ex)
        {
            var title = l[ex.Code];
            await Write(context, StatusCodes.Status400BadRequest,
                new ProblemDetails { Title = title.ResourceNotFound ? ex.Message : title.Value, Detail = ex.Code, Status = 400 });
        }
        catch (ConflictException ex)
        {
            await Write(context, StatusCodes.Status409Conflict, new ProblemDetails { Title = l.Text(ex.Code), Detail = ex.Code, Status = 409 });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Requisição cancelada pelo cliente: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
            await Write(context, StatusCodes.Status500InternalServerError,
                new ProblemDetails { Title = l.Text("Server.Unexpected"), Detail = "Server.Unexpected", Status = 500 });
        }
    }

    private static string ToCamel(string name) => string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];

    private static async Task Write(HttpContext context, int status, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
            return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
    }
}
