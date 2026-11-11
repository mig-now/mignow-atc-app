using System.Text;
using FluentAssertions;
using Mignow.Atc.Application.Modules.Extraction;

namespace Mignow.Atc.Tests.Extraction;

/// <summary>Paridade com o <c>converter.py</c> original (Golden/extraction): mesmos bytes do CSV de remediação.</summary>
public sealed class RemediationCsvGoldenTests
{
    private static string Golden(string name) => Path.Combine(AppContext.BaseDirectory, "Golden", "extraction", name);

    [Fact]
    public void Layout_de_remediacao_gera_os_mesmos_bytes_do_converter_py()
    {
        var rows = ParseCsv(File.ReadAllText(Golden("novos_input.csv"), new UTF8Encoding(true)).TrimStart('﻿'));
        var header = rows[0];
        var dicts = rows.Skip(1).Select(r => (IReadOnlyDictionary<string, string>)header.Zip(r).ToDictionary(x => x.First, x => x.Second)).ToList();
        RemediationCsvLayout.Write(dicts).Should().Equal(File.ReadAllBytes(Golden("expected_remediacao.csv")));
    }

    [Fact]
    public void Csv_total_entre_aspas_em_todos_os_campos_com_bom_e_crlf()
    {
        var bytes = ExtractionCsv.WriteQuotedAll(["a", "b"], [new Dictionary<string, string> { ["a"] = "x\"y", ["b"] = "1;2" }, new Dictionary<string, string> { ["a"] = "z" }]);
        Encoding.UTF8.GetString(bytes).Should().Be("﻿\"a\";\"b\"\r\n\"x\"\"y\";\"1;2\"\r\n\"z\";\"\"\r\n");
    }

    /// <summary>Leitor mínimo de CSV (';', aspas duplas, quebras de linha dentro de aspas).</summary>
    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else field.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == ';') { row.Add(field.ToString()); field.Clear(); }
            else if (c == '\r') { }
            else if (c == '\n') { row.Add(field.ToString()); field.Clear(); rows.Add(row); row = []; }
            else field.Append(c);
        }
        return rows;
    }
}
