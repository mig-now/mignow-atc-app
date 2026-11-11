namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Falha da KB (categoria inválida, trava de arquivo, pasta ausente). <see cref="Code"/> = chave do <c>KnowledgeErrors.resx</c>.</summary>
public sealed class KbException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
