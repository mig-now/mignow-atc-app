using System.Text;

namespace Mignow.Atc.Application.Modules.Knowledge;

/// <summary>Slugs e utilidades de texto da KB.</summary>
public static class KbText
{
    /// <summary>Porte de <c>re.sub(r"[^A-Za-z0-9_]", "_", s.upper())</c> (um "_" por caractere Unicode, não por UTF-16).</summary>
    public static string Slug(string element)
    {
        var sb = new StringBuilder();
        foreach (var r in element.ToUpperInvariant().EnumerateRunes())
            sb.Append(r.Value is < 128 && (char.IsAsciiLetterOrDigit((char)r.Value) || r.Value == '_') ? (char)r.Value : '_');
        return sb.ToString();
    }

    /// <summary>Equivalente do <c>fato_slug</c>: <c>strip()</c> + slug.</summary>
    public static string FactSlug(string element) => Slug(element.Trim());

    /// <summary>Quebra de linhas do <c>str.splitlines()</c> do Python.</summary>
    public static string[] SplitLines(string text)
    {
        if (text.Length == 0)
            return [];
        var lines = new List<string>();
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c is '\n' or '\r' or '\v' or '\f' or '\u001c' or '\u001d' or '\u001e' or '\u0085' or '\u2028' or '\u2029')
            {
                lines.Add(text[start..i]);
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                start = i + 1;
            }
        }
        if (start < text.Length)
            lines.Add(text[start..]);
        return [.. lines];
    }

    /// <summary>Leitura "universal newlines" do Python (CRLF/CR → LF).</summary>
    public static string NormalizeNewlines(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n');
}
