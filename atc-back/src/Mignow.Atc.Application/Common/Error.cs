namespace Mignow.Atc.Application.Common;

/// <summary>Erro de negócio. <see cref="Code"/> é a chave estável (vai no <c>detail</c> do ProblemDetails; o front lê).</summary>
/// <remarks><see cref="Data"/>: detalhe extra no ProblemDetails (ex.: o run que bloqueia o ambiente no 409).</remarks>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure, object? Data = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string code, string msg) => new(code, msg, ErrorType.NotFound);

    public static Error Conflict(string code, string msg) => new(code, msg, ErrorType.Conflict);

    public static Error Validation(string code, string msg) => new(code, msg, ErrorType.Validation);

    public static Error Upstream(string code, string msg) => new(code, msg, ErrorType.Upstream);

    public static Error Forbidden(string code, string msg) => new(code, msg, ErrorType.Forbidden);

    public static Error Failure(string code, string msg) => new(code, msg, ErrorType.Failure);
}
