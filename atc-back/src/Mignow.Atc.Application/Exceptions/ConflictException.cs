namespace Mignow.Atc.Application.Exceptions;

/// <summary>Violação de unicidade no banco (corrida entre requisições/réplicas) → 409 no middleware.</summary>
public sealed class ConflictException(string code, string message, Exception? inner = null) : Exception(message, inner)
{
    public string Code { get; } = code;
}
