namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Marcadores de código novo (porte de <c>marker()</c>/<c>cmd_marcadores</c>, decisão 7):
/// <c>*&gt;&gt;&gt; S/4 MIGRATION MIG-NOW - START OF DELETION - DD.MM.YYYY AUT-XX</c> e o <c>END</c> com <c>*&lt;&lt;&lt;</c>.
/// <c>delecao</c> comenta o código atual (prefixa <c>*</c>), <c>insercao</c> só o novo, <c>modificacao</c> = deleção + inserção.
/// </summary>
public static class ChangeMarker
{
    public const string Company = "MIG-NOW";

    /// <summary>Autor como o <c>configurar</c> grava: iniciais em maiúsculas com o prefixo <c>AUT-</c>.</summary>
    public static string AuthorTag(string author)
    {
        var a = author.Trim().ToUpperInvariant();
        return a.StartsWith("AUT-", StringComparison.Ordinal) ? a : "AUT-" + a;
    }

    public static string Marker(string authorTag, string startOrEnd, string kind, DateOnly date)
    {
        var arrow = startOrEnd == "START" ? ">>>" : "<<<";
        return $"*{arrow} S/4 MIGRATION {Company} - {startOrEnd} OF {kind} - {date:dd.MM.yyyy} {authorTag}";
    }

    /// <summary>
    /// Linhas do bloco. Erros (código estável): <c>Apply.Marker.InvalidKind</c>, <c>Apply.Marker.BeforeRequired</c> (delecao/modificacao),
    /// <c>Apply.Marker.AfterRequired</c> (insercao/modificacao).
    /// </summary>
    public static MarkerResult Build(string kind, IReadOnlyList<string> before, IReadOnlyList<string> after, string authorTag, DateOnly date)
    {
        if (!MarkerKind.IsValid(kind))
            return MarkerResult.Fail("Apply.Marker.InvalidKind", kind);
        var deletes = kind is MarkerKind.Deletion or MarkerKind.Modification;
        var inserts = kind is MarkerKind.Insertion or MarkerKind.Modification;
        if (deletes && before.Count == 0)
            return MarkerResult.Fail("Apply.Marker.BeforeRequired", kind);
        if (inserts && after.Count == 0)
            return MarkerResult.Fail("Apply.Marker.AfterRequired", kind);
        List<string> lines = [];
        if (deletes)
        {
            lines.Add(Marker(authorTag, "START", "DELETION", date));
            lines.AddRange(before.Select(l => "*" + l));
            lines.Add(Marker(authorTag, "END", "DELETION", date));
        }
        if (inserts)
        {
            lines.Add(Marker(authorTag, "START", "INSERTION", date));
            lines.AddRange(after);
            lines.Add(Marker(authorTag, "END", "INSERTION", date));
        }
        return MarkerResult.Ok(lines);
    }
}
