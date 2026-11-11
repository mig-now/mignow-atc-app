using System.Text;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// CSVs idênticos aos da skill: <c>;</c>, UTF-8 com BOM, quebra de linha CRLF. O total e o <c>_novos</c> saem como o
/// <c>Write-CsvFile</c> do PowerShell (TODOS os campos entre aspas); o de remediação como o <c>csv.writer</c> do Python
/// (aspas só quando o campo tem <c>;</c>, <c>"</c>, CR ou LF).
/// </summary>
public static class ExtractionCsv
{
    private const string NewLine = "\r\n";

    public static byte[] WriteQuotedAll(IReadOnlyList<string> columns, IEnumerable<IReadOnlyDictionary<string, string>> rows)
    {
        var sb = new StringBuilder();
        Line(sb, columns, Quote);
        foreach (var r in rows)
            Line(sb, columns.Select(c => ExtractionTableBuilder.Value(r, c)), Quote);
        return Encode(sb);
    }

    public static byte[] WriteMinimal(IReadOnlyList<string> header, IEnumerable<IReadOnlyList<string>> rows)
    {
        var sb = new StringBuilder();
        Line(sb, header, Minimal);
        foreach (var r in rows)
            Line(sb, r, Minimal);
        return Encode(sb);
    }

    private static byte[] Encode(StringBuilder sb) => [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(sb.ToString())];

    private static void Line(StringBuilder sb, IEnumerable<string> fields, Func<string, string> format)
    {
        sb.Append(string.Join(';', fields.Select(format))).Append(NewLine);
    }

    private static string Quote(string v) => '"' + v.Replace("\"", "\"\"", StringComparison.Ordinal) + '"';

    private static string Minimal(string v) => v.AsSpan().IndexOfAny(";\"\r\n") >= 0 ? Quote(v) : v;
}
