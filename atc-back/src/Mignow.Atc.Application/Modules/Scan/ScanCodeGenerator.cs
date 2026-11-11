using System.Globalization;
using System.Text;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// Porte de <c>gerar.py</c>: gera o ABAP da classe temporária de cada lote a partir do template, das regras ativas e dos
/// objetos do lote (mesmo texto byte a byte; a data da geração é parâmetro). Literais ABAP em pedaços de 150 caracteres
/// unidos por <c>&amp;&amp;</c> (linha ABAP ≤ 255).
/// </summary>
public static class ScanCodeGenerator
{
    public const string DefaultClass = "ZCL_MIGNOW_ATC_SCAN";
    public const int MaxAbapLine = 255;

    /// <summary>Código ABAP do lote. <paramref name="classe"/> já em maiúsculas; falha com <see cref="ScanGenerationException"/> se a linha passar de 255.</summary>
    public static string Generate(string classe, int lote, IReadOnlyList<(string Type, string Name)> objects, MignowRulesDocument rules, bool compare, DateTime generatedAt)
    {
        var carregar = LoadBlock(rules, compare);
        var src = (ScanClassTemplate.Text + "\n")
            .Replace("{{CLASSE}}", classe, StringComparison.Ordinal)
            .Replace("{{LOTE}}", lote.ToString("D3", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{{GERADA_EM}}", generatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{{COMPARAR}}", compare ? "X" : " ", StringComparison.Ordinal)
            .Replace("{{CARREGAR}}", carregar, StringComparison.Ordinal)
            .Replace("{{ESCOPO}}", ScopeBlock(objects), StringComparison.Ordinal);
        var lines = src.Split('\n');
        var tooLong = lines.Select((l, i) => (l, n: i + 1)).Where(x => x.l.Length > MaxAbapLine).Select(x => x.n).Take(10).ToList();
        if (tooLong.Count > 0)
            throw new ScanGenerationException(lote, tooLong);
        return src;
    }

    /// <summary>Literal ABAP de string (<c>`…`</c>) em pedaços de até 150 caracteres unidos por <c>&amp;&amp;</c>.</summary>
    internal static string Lit(string s, int indent)
    {
        s = s.Replace("`", "``", StringComparison.Ordinal);
        var pieces = new List<string>();
        for (var i = 0; i < s.Length; i += 150)
            pieces.Add(s.Substring(i, Math.Min(150, s.Length - i)));
        if (pieces.Count == 0)
            pieces.Add(string.Empty);
        return string.Join(" &&\n" + new string(' ', indent), pieces.Select(p => "`" + p + "`"));
    }

    internal static string LoadBlock(MignowRulesDocument cfg, bool compare)
    {
        const string ind = "    ";
        var ls = new List<string> { $"{ind}mv_gatilho = {Lit(cfg.Trigger, 18)}." };
        foreach (var r in cfg.ActiveRules)
        {
            var ex = r.Requires.Length > 0 ? "(?i)" + r.Requires : string.Empty;
            ls.Add($"{ind}APPEND VALUE #( id = `{r.Id}` texto = '{r.Text}' elem = `{r.FixedElement}`\n"
                   + $"{ind}                exige = {Lit(ex, 24)}\n"
                   + $"{ind}                supr = `{r.SuppressedBy}`\n"
                   + $"{ind}                pcre = {Lit("(?i)" + r.Pcre, 23)} ) TO mt_regra.");
        }
        if (compare)
        {
            ls.Add($"{ind}IF c_comparar = abap_true.");
            foreach (var r in cfg.ActiveRules)
                ls.Add($"{ind}  APPEND VALUE #( id = `{r.Id}`\n{ind}                  pcre = {Lit(r.OriginalPcre, 25)} ) TO mt_orig.");
            ls.Add($"{ind}ENDIF.");
        }
        return string.Join('\n', ls);
    }

    internal static string ScopeBlock(IReadOnlyList<(string Type, string Name)> objects)
    {
        const string ind = "    ";
        var sb = new StringBuilder();
        sb.Append(ind).Append("rt = VALUE #(\n");
        sb.Append(string.Join('\n', objects.Select(o => $"{ind}  ( tipo = '{o.Type.Replace("'", "''", StringComparison.Ordinal)}' nome = '{o.Name.Replace("'", "''", StringComparison.Ordinal)}' )")));
        sb.Append('\n').Append(ind).Append(").\n");
        return sb.ToString();
    }
}
