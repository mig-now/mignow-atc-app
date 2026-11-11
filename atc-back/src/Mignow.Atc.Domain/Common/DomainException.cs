namespace Mignow.Atc.Domain.Common;

/// <summary>Invariante de domínio violada. <see cref="Code"/> é a chave estável (i18n).</summary>
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
