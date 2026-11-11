namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Texto como o Python das skills lê (<c>str.splitlines</c>, <c>rstrip</c>): a base do porte fiel do <c>marcadores</c> e do <c>preparar</c>.</summary>
public static class PyText
{
    private static readonly char[] LineBreaks = [.. new[] { 10, 13, 11, 12, 0x1c, 0x1d, 0x1e, 0x85, 0x2028, 0x2029 }.Select(c => (char)c)];

    /// <summary><c>str.splitlines()</c>: quebra em \n, \r\n, \r (e os demais separadores do Python) sem manter o terminador.</summary>
    public static List<string> SplitLines(string text)
    {
        List<string> lines = [];
        var start = 0;
        var i = 0;
        while (i < text.Length)
        {
            if (Array.IndexOf(LineBreaks, text[i]) < 0)
            {
                i++;
                continue;
            }
            lines.Add(text[start..i]);
            i += text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n' ? 2 : 1;
            start = i;
        }
        if (start < text.Length)
            lines.Add(text[start..]);
        return lines;
    }

    /// <summary>Linhas de um arquivo de trecho (<c>read_text().rstrip("\n").splitlines()</c>).</summary>
    public static List<string> LinesOfFile(string text) => SplitLines(text.TrimEnd('\n'));

    /// <summary>As linhas já sem espaços no fim (<c>norm</c> do <c>cmd_preparar</c>).</summary>
    public static List<string> Normalize(IEnumerable<string> lines) => [.. lines.Select(l => l.TrimEnd())];
}
