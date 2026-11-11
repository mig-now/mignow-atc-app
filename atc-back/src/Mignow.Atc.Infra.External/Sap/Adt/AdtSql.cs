using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mignow.Atc.Application.Ports.Sap;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Texto e resposta do data preview (porte de <c>Format-AdtSql</c> e <c>Invoke-AdtSqlRaw</c> de SapSql.psm1): o ADT
/// recusa linhas com mais de 255 caracteres, então quebra antes de AND/OR/WHERE/FROM/ORDER BY/GROUP BY/INNER/LEFT e nas
/// vírgulas de listas longas (IN). Só SELECT.
/// </summary>
internal static partial class AdtSql
{
    public const string DataPreviewNs = "http://www.sap.com/adt/dataPreview";

    public static bool IsSelect(string sql) => SelectRegex().IsMatch(sql) && !WriteRegex().IsMatch(sql);

    public static string Format(string sql)
    {
        var text = BreakRegex().Replace(sql, "\n").Trim();
        var lines = new List<string>();
        foreach (var raw in text.Split('\n'))
        {
            var l = raw;
            while (l.Length > 240)
            {
                var p = l.LastIndexOf(',', 240);
                if (p < 1)
                    break;
                lines.Add(l[..(p + 1)]);
                l = l[(p + 1)..];
            }
            lines.Add(l);
        }
        return string.Join('\n', lines);
    }

    /// <summary>Colunas (dataPreview:columns → metadata/@name) e valores (dataSet/data, sem espaços nas pontas).</summary>
    public static SqlResult Parse(string xml)
    {
        var doc = XDocument.Parse(xml);
        XNamespace d = DataPreviewNs;
        var columns = doc.Descendants(d + "columns").ToList();
        var names = columns.Select(c => c.Element(d + "metadata")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "name")?.Value ?? string.Empty).ToList();
        var data = columns.Select(c => c.Element(d + "dataSet")?.Elements(d + "data").Select(x => x.Value.Trim()).ToList() ?? []).ToList();
        var count = data.Count == 0 ? 0 : data[0].Count;
        var rows = new List<IReadOnlyDictionary<string, string>>(count);
        for (var i = 0; i < count; i++)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var k = 0; k < names.Count; k++)
                row[names[k]] = i < data[k].Count ? data[k][i] : string.Empty;
            rows.Add(row);
        }
        return new SqlResult(names, rows);
    }

    /// <summary>Lista para <c>IN (...)</c>: valores distintos, ordenados, com aspas simples escapadas.</summary>
    public static IEnumerable<string> InLists(IEnumerable<string> values, int blockSize)
    {
        var v = values.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        for (var i = 0; i < v.Count; i += blockSize)
            yield return string.Join(", ", v.Skip(i).Take(blockSize).Select(x => "'" + x.Replace("'", "''") + "'"));
    }

    [GeneratedRegex(@"^\s*SELECT\s", RegexOptions.IgnoreCase)]
    private static partial Regex SelectRegex();

    [GeneratedRegex(@"\b(INSERT|UPDATE|DELETE|MODIFY)\b\s", RegexOptions.IgnoreCase)]
    private static partial Regex WriteRegex();

    [GeneratedRegex(@"\s+(?=(?i:AND|OR|WHERE|FROM|ORDER\s+BY|GROUP\s+BY|INNER|LEFT)\s)")]
    private static partial Regex BreakRegex();
}
