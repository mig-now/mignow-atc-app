using System.Text;

namespace Mignow.Atc.Application.Modules.Knowledge;

/// <summary>Geração do markdown das contribuições e fatos (formato exato do <c>cmd_registrar</c>/<c>cmd_fato</c> do atc.py).</summary>
public static class KbMarkdownWriter
{
    public static string ContributionHeader(string element, string category) =>
        $"# {element.ToUpperInvariant()}\n\n**Categoria:** {category}  \n"
        + "Soluções validadas em sessões assistidas (análise IA confirmada pelo consultor).\n\n"
        + "[← Contribuições](../README.md)\n\n---\n\n";

    public static string ContributionCard(string id, string date, string? problem, IReadOnlyList<string> replaces, string before, string after)
    {
        var lines = new List<string>
        {
            $"<a id=\"{id.ToLowerInvariant()}\"></a>", $"### {id}", "",
            "**Padrão:** Contribuição (IA confirmada)  ", "**Ocorrências:** 1  ",
            $"**Registrado em:** {date}  ",
        };
        if (!string.IsNullOrEmpty(problem))
            lines.Add($"**Problema:** {problem}  ");
        if (replaces.Count > 0)
            lines.Add($"**Substitui:** {string.Join(", ", replaces)}  ");
        lines.AddRange(["", "**Problema (código original)**", "", "```abap", TrimEnd(before), "```", "",
            "**Solução (código novo)**", "", "```abap", TrimEnd(after), "```", "", "---", ""]);
        return string.Join("\n", lines);
    }

    public static string FactHeader(string slug) =>
        $"# {slug} – fatos\n\nFatos técnicos verificados nas sessões de remediação (comportamento de API/classe, "
        + "pré-requisitos, armadilhas). Leia antes de propor correção para este elemento.\n\n"
        + "[← Fatos](README.md)\n\n---\n\n";

    public static string FactCard(string id, string date, string fact, string? proof, string? impact)
    {
        var lines = new List<string> { $"<a id=\"{id.ToLowerInvariant()}\"></a>", $"### {id}", "", $"**Registrado em:** {date}  ", $"**Fato:** {fact}  " };
        if (!string.IsNullOrEmpty(proof))
            lines.Add($"**Prova:** {proof}  ");
        if (!string.IsNullOrEmpty(impact))
            lines.Add($"**Impacto/como usar:** {impact}  ");
        lines.AddRange(["---", "", ""]);
        return string.Join("\n", lines);
    }

    /// <summary>README de <c>contribuicoes/</c>; linhas = (elemento, pasta, nº de soluções), já ordenadas.</summary>
    public static string ContributionsReadme(IEnumerable<(string Stem, string Folder, int Count)> rows) =>
        string.Join("\n", new[]
        {
            "# Contribuições", "",
            "Soluções novas registradas pela skill `atc-remediacao` (análise IA confirmada). "
            + "Não são sobrescritas quando a base é regerada.", "",
            "| Elemento | Categoria | Soluções |", "|---|---|---|",
        }.Concat(rows.Select(r => $"| [{r.Stem}]({r.Folder}/{r.Stem}.md) | {r.Folder} | {r.Count} |")).Append(""));

    /// <summary>README de <c>contribuicoes/fatos/</c>; linhas = (elemento, nº de fatos), já ordenadas.</summary>
    public static string FactsReadme(IEnumerable<(string Stem, int Count)> rows) =>
        string.Join("\n", new[]
        {
            "# Fatos da KB", "",
            "Fatos técnicos reaproveitáveis registrados pela skill `atc-remediacao` (`atc.py fato`). "
            + "Não são sobrescritos quando a base é regerada. O `atc.py item` mostra os fatos do objeto referenciado.", "",
            "| Elemento | Fatos |", "|---|---|",
        }.Concat(rows.Select(r => $"| [{r.Stem}]({r.Stem}.md) | {r.Count} |")).Append(""));

    /// <summary><c>read_text().rstrip()</c> com universal newlines.</summary>
    private static string TrimEnd(string s) => KbText.NormalizeNewlines(s).TrimEnd();
}
