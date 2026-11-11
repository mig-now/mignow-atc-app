using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Knowledge;

/// <summary>Cartão bruto lido do markdown (<c>parse_cards</c>).</summary>
public sealed record RawCard(string Id, int Occurrences, string Pattern, string Original, string New);

/// <summary>Porte de <c>parse_cards</c> e <c>fatos_de</c> do atc.py.</summary>
public static partial class KbCardParser
{
    [GeneratedRegex("<a id=\"[^\"]+\"></a>\\s*\\n")]
    private static partial Regex AnchorSplit();

    [GeneratedRegex(@"^### (\S+)", RegexOptions.Multiline)]
    private static partial Regex IdRx();

    [GeneratedRegex(@"\*\*Ocorrências:\*\*\s*(\d+)")]
    private static partial Regex OccRx();

    [GeneratedRegex(@"\*\*Padrão:\*\*\s*(.+)")]
    private static partial Regex PatternRx();

    [GeneratedRegex("```abap\\n(.*?)\\n```", RegexOptions.Singleline)]
    private static partial Regex CodeRx();

    [GeneratedRegex(@"\[([^\]]+)\]\([^)]*\)")]
    private static partial Regex LinkRx();

    [GeneratedRegex(@"^### (F-\S+)\n\n\*\*Registrado em:\*\* (\S+)  \n(.*?)\n---", RegexOptions.Singleline | RegexOptions.Multiline)]
    private static partial Regex FactRx();

    public static IReadOnlyList<RawCard> Parse(string md)
    {
        var cards = new List<RawCard>();
        var blocks = AnchorSplit().Split(md);
        foreach (var blk in blocks.Skip(1))
        {
            var id = IdRx().Match(blk);
            var occ = OccRx().Match(blk);
            var pat = PatternRx().Match(blk);
            var codes = CodeRx().Matches(blk);
            if (!id.Success || codes.Count < 2)
                continue;
            var occurrences = occ.Success && int.TryParse(occ.Groups[1].Value, out var n) ? n : 1;
            cards.Add(new RawCard(id.Groups[1].Value, occurrences,
                pat.Success ? LinkRx().Replace(pat.Groups[1].Value, "$1").Trim() : "",
                codes[0].Groups[1].Value, codes[1].Groups[1].Value));
        }
        return cards;
    }

    /// <summary>Fatos de um arquivo <c>contribuicoes/fatos/&lt;ELEM&gt;.md</c> (id, data, texto).</summary>
    public static IReadOnlyList<(string Id, string Date, string Text)> ParseFacts(string md) =>
        [.. FactRx().Matches(md).Select(m => (m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value.Trim()))];

    /// <summary>Título do arquivo (<c># NOME</c> da primeira linha), usado como nome de exibição do elemento.</summary>
    public static string? Heading(string md)
    {
        var first = md.Split('\n', 2)[0];
        return first.StartsWith("# ", StringComparison.Ordinal) ? first[2..].Trim() : null;
    }
}
