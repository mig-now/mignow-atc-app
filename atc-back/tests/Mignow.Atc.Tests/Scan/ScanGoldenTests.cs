using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using FluentAssertions;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Tests.Scan;

/// <summary>Paridade com gerar.py e cruzar.py (Golden/scan: entradas sintéticas + saída do script original, python3 -I).</summary>
public sealed partial class ScanGoldenTests
{
    private static readonly string Golden = Path.Combine(AppContext.BaseDirectory, "Golden", "scan");

    private static string Read(params string[] parts) => File.ReadAllText(Path.Combine([Golden, .. parts]), new UTF8Encoding(false));

    [Theory]
    [InlineData("comparar", true)]
    [InlineData("sem_comparar", false)]
    public void Gerador_produz_o_mesmo_abap_do_gerar_py(string variant, bool compare)
    {
        var rules = MignowRulesDocument.Parse(Read("regras.json"));
        using var lots = JsonDocument.Parse(Read("gerar", "lotes.json"));
        foreach (var lot in lots.RootElement.EnumerateArray())
        {
            var n = lot.GetProperty("lote").GetInt32();
            var objects = lot.GetProperty("objetos").EnumerateArray().Select(o => (o[0].GetString()!, o[1].GetString()!)).ToList();
            var src = ScanCodeGenerator.Generate("ZCL_MIGNOW_ATC_SCAN", n, objects, rules, compare, new DateTime(2026, 11, 8, 19, 50, 0));
            var expected = Read("gerar", $"esperado_{variant}", $"lote_{n:D3}.abap");
            Normalize(src).Should().Be(Normalize(expected), $"lote {n} ({variant})");
        }
    }

    [Fact]
    public void Gerador_recusa_linha_abap_acima_de_255()
    {
        var doc = MignowRulesDocument.Parse(Read("regras.json"));
        var bad = doc with { Trigger = "X" };
        var objects = new List<(string, string)> { (new string('A', 300), "N") };
        var act = () => ScanCodeGenerator.Generate("ZCL_X", 1, objects, bad, false, DateTime.Now);
        act.Should().Throw<ScanGenerationException>();
    }

    [Theory]
    [InlineData("padrao", false)]
    [InlineData("outra_linha", true)]
    public void Cruzamento_igual_ao_cruzar_py(string variant, bool includeOtherLine)
    {
        var entrada = Path.Combine("cruzar", "entrada");
        var expectedSummary = JsonNode.Parse(Read("cruzar", $"esperado_{variant}", "resumo.json"))!.AsObject();
        var now = DateTime.ParseExact(expectedSummary["gerado_em"]!.GetValue<string>(), "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        var rules = MignowRulesDocument.Parse(Read("regras.json"));
        var scope = ReadCsv(Read(entrada, "escopo.csv")).Skip(1).ToList();
        var inventory = JsonNode.Parse(Read(entrada, "inventario.json"))!.AsObject();

        var occurrences = new List<MignowOccurrence>();
        var batches = new List<ScanBatch>();
        foreach (var file in Directory.GetFiles(Path.Combine(Golden, entrada, "saida"), "lote_*.txt").Order(StringComparer.Ordinal))
        {
            var n = int.Parse(Path.GetFileNameWithoutExtension(file)[5..], CultureInfo.InvariantCulture);
            var parsed = ScanOutputParser.Parse(File.ReadAllText(file));
            var seen = new HashSet<(string, int, string, string)>();
            foreach (var h in parsed.Hits.Where(h => seen.Add((h.Source, h.Line, h.RuleId, h.Element))))
                occurrences.Add(MignowOccurrence.Create(1, h.RuleId, h.Name, h.Type, null, h.Source, h.Line, h.LineEnd, h.Snippet, h.Element, n, h.Flag));
            var objs = scope.Where(r => r[4] == n.ToString(CultureInfo.InvariantCulture)).Select(r => new ScanObject(r[0], r[1], r[2], r[3])).ToList();
            batches.Add(new ScanBatch(n, 0, objs, "ZCL_MIGNOW_ATC_SCAN", ScanBatchStatus.Ok, true, parsed.ToStats(null, null, [], null)));
        }
        var findings = ReadCsv(Read(entrada, "atc_total.csv")).Skip(1).Select(r => new ScanAtcFinding(r[1], r[2])).ToList();
        var byType = inventory["por_tipo"]!.AsObject().ToDictionary(kv => kv.Key, kv => kv.Value!.GetValue<int>());
        var ex = inventory["excluidos"]!;
        var input = new ScanCrossInput(occurrences, batches, new ScanInventoryCounts(inventory["escopo"]!.GetValue<int>(), byType, ex["tmp"]!.GetValue<int>(),
            ex["marcados_exclusao"]!.GetValue<int>(), ex["gerados"]!.GetValue<int>()), rules, findings, new ScanAtcBase("atc_total.csv", null, null),
            expectedSummary["varredura"]!.GetValue<string>(), "TST", "100", "ZCL_MIGNOW_ATC_SCAN", includeOtherLine, now);

        var result = ScanCrossAnalyzer.Run(input);

        // ocorrencias.csv: situação e cruzamento de cada ocorrência
        var expectedRows = ReadCsv(Read("cruzar", $"esperado_{variant}", "ocorrencias.csv")).Skip(1).ToList();
        occurrences.Should().HaveCount(expectedRows.Count);
        for (var i = 0; i < expectedRows.Count; i++)
        {
            var o = occurrences[i];
            var r = expectedRows[i];
            (o.ObjectType, o.ObjectName, o.AdtSource, o.Line.ToString(CultureInfo.InvariantCulture), o.LineEnd!.Value.ToString(CultureInfo.InvariantCulture), o.RuleId, o.Element,
                ScanCrossAnalyzer.Situation(o.Class), o.Cross ?? string.Empty, o.Snippet)
                .Should().Be((r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[8], r[9]), $"ocorrência {i + 1}");
        }

        // CSV P1: mesmos bytes (nome com o carimbo da execução original)
        var expectedFile = Directory.GetFiles(Path.Combine(Golden, "cruzar", $"esperado_{variant}"), "mignow_*_remediacao.csv").Single();
        result.P1FileName.Should().Be(Path.GetFileName(expectedFile));
        var selected = ScanP1Csv.Select(occurrences, includeOtherLine);
        var csv = ScanP1Csv.Write(selected, ScanCrossAnalyzer.ScopeLookup(batches), rules.Rules.ToDictionary(r => r.Id), now);
        csv.Should().Equal(File.ReadAllBytes(expectedFile));

        AssertJson(JsonNode.Parse(result.Summary.ToJsonString())!, expectedSummary, "$");
        result.Markdown.Should().Be(Read("cruzar", $"esperado_{variant}", "resumo.md"));
    }

    private static void AssertJson(JsonNode actual, JsonNode expected, string path)
    {
        switch (expected)
        {
            case JsonObject eo:
                var ao = actual.Should().BeOfType<JsonObject>(path).Subject;
                ao.Select(k => k.Key).Should().BeEquivalentTo(eo.Select(k => k.Key), path);
                foreach (var (k, v) in eo)
                    if (v is null)
                        ao[k].Should().BeNull($"{path}.{k}");
                    else
                        AssertJson(ao[k]!, v, $"{path}.{k}");
                break;
            case JsonArray ea:
                var aa = actual.Should().BeOfType<JsonArray>(path).Subject;
                aa.Count.Should().Be(ea.Count, path);
                for (var i = 0; i < ea.Count; i++)
                    AssertJson(aa[i]!, ea[i]!, $"{path}[{i}]");
                break;
            default:
                var ev = ((JsonValue)expected).GetValue<JsonElement>();
                var av = ((JsonValue)actual).GetValue<JsonElement>();
                if (ev.ValueKind == JsonValueKind.Number)
                    av.GetDouble().Should().Be(ev.GetDouble(), path);
                else
                    av.ToString().Should().Be(ev.ToString(), path);
                break;
        }
    }

    private static string Normalize(string abap) => GeneratedAt().Replace(abap, "gerada em <data>");

    /// <summary>CSV simples do Python (; e aspas), como o <c>csv.reader</c>.</summary>
    private static List<List<string>> ReadCsv(string text)
    {
        text = text.TrimStart('\uFEFF');
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                    quoted = false;
                else
                    field.Append(c);
            }
            else if (c == '"')
                quoted = true;
            else if (c == ';')
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                row.Add(field.ToString());
                field.Clear();
                rows.Add(row);
                row = [];
            }
            else
                field.Append(c);
        }
        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }

    [GeneratedRegex(@"gerada em \d\d\.\d\d\.\d{4} \d\d:\d\d")]
    private static partial Regex GeneratedAt();
}
