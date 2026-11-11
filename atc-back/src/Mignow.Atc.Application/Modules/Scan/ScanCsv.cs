using System.Text;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>CSV como o módulo <c>csv</c> do Python (delimitador <c>;</c>, aspas mínimas, linhas <c>\r\n</c>) em UTF-8 com BOM (<c>utf-8-sig</c>).</summary>
internal static class ScanCsv
{
    public static byte[] Write(IReadOnlyList<string> header, IEnumerable<IReadOnlyList<string>> rows)
    {
        var sb = new StringBuilder();
        Row(sb, header);
        foreach (var r in rows)
            Row(sb, r);
        return [.. Encoding.UTF8.GetPreamble(), .. new UTF8Encoding(false).GetBytes(sb.ToString())];
    }

    private static void Row(StringBuilder sb, IReadOnlyList<string> fields)
    {
        for (var i = 0; i < fields.Count; i++)
        {
            if (i > 0)
                sb.Append(';');
            var f = fields[i];
            if (f.AsSpan().IndexOfAny(";\"\r\n") >= 0)
                sb.Append('"').Append(f.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"');
            else
                sb.Append(f);
        }
        sb.Append("\r\n");
    }
}
