namespace Mignow.Atc.Application.Common;

/// <summary>Envelope de SUCESSO <c>{ success, data, message }</c> (erros saem como ProblemDetails). Não mudar o shape.</summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public string? Message { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new() { Success = true, Data = data, Message = message };
}
