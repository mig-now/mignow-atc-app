namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Tipo do bloco de correção (<c>--tipo</c> do <c>marcadores</c>): <c>modificacao</c>, <c>delecao</c> ou <c>insercao</c>.</summary>
public static class MarkerKind
{
    public const string Modification = "modificacao";
    public const string Deletion = "delecao";
    public const string Insertion = "insercao";

    public static bool IsValid(string? kind) => kind is Modification or Deletion or Insertion;
}
