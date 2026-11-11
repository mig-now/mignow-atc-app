using System.Globalization;
using System.Text.Json.Nodes;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// Porte de <c>cruzar.py</c> (sem arquivos): classifica cada ocorrência pendente pela <c>location</c> do ATC e pelo
/// Referenced Object — ATC_PEGOU, ATC_OUTRA_LINHA, MIGNOW_FONTE_COM_FINDING, MIGNOW_FONTE_SEM_FINDING; já remediadas (R) e
/// informativas (I) só são contadas — e monta o resumo (<c>resumo.json</c>/<c>.md</c>). Ocorrências duplicadas (mesmo fonte,
/// linha, regra e elemento) contam uma vez.
/// </summary>
public static class ScanCrossAnalyzer
{
    public static ScanCrossResult Run(ScanCrossInput input)
    {
        var index = new ScanAtcIndex(input.Findings);
        var rules = input.Rules.Rules.ToDictionary(r => r.Id, StringComparer.Ordinal);
        var scope = ScopeLookup(input.Batches);

        // 1) cruzamento (só dos pendentes) – ocorrências duplicadas já foram removidas na leitura da saída
        var occ = input.Occurrences;
        foreach (var o in occ)
        {
            if (o.Class != ScanOccurrenceStatus.Pending)
                continue;
            var source = o.AdtSource ?? string.Empty;
            var comparable = !source.Contains('#', StringComparison.Ordinal);
            var found = index.For(source);
            var same = found.Where(f => f.Ref == o.Element.ToUpperInvariant()).ToList();
            var ini = o.Line;
            var fim = o.LineEnd ?? o.Line;
            string cross;
            if (comparable && same.Any(f => f.Line is { } l && ini <= l && l <= fim))
                cross = ScanCross.AtcCaught;
            else if (same.Count > 0)
                cross = ScanCross.AtcOtherLine;
            else if (found.Count > 0)
                cross = ScanCross.SourceWithFinding;
            else
                cross = ScanCross.SourceWithoutFinding;
            o.SetCross(cross);
        }

        var pend = occ.Where(o => o.Class == ScanOccurrenceStatus.Pending).ToList();
        var mig = pend.Where(o => ScanP1Csv.MignowClasses.Contains(o.Cross!)).ToList();
        var toCsv = ScanP1Csv.Select(occ, input.IncludeOtherLine);
        var csvName = ScanP1Csv.FileName(input.Sid, input.Client, input.Now);

        // 2) estatísticas
        var stats = input.Batches.Select(b => b.Stats).Where(s => s is not null).Select(s => s!).ToList();
        var original = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var s in stats)
            foreach (var (rid, q) in s.OriginalByRule)
                original[rid] = original.GetValueOrDefault(rid) + q;
        var origTotal = original.Values.Sum();

        var porRegra = new JsonObject();
        foreach (var rid in rules.Keys.Order(StringComparer.Ordinal))
        {
            var oo = occ.Where(o => o.RuleId == rid).ToList();
            if (oo.Count == 0 && original.GetValueOrDefault(rid) == 0)
                continue;
            var item = new JsonObject
            {
                ["descricao"] = Truncate(rules[rid].Description, 90),
                ["original_linhas"] = original.GetValueOrDefault(rid),
                ["refinada_total"] = oo.Count,
            };
            foreach (var (k, v) in Count(oo.Select(o => Situation(o.Class))))
                item[k] = v;
            foreach (var (k, v) in Count(oo.Select(o => o.Cross ?? string.Empty).Where(c => c.Length > 0)))
                item[k] = v;
            porRegra[rid] = item;
        }

        var coverage = new JsonObject();
        foreach (var (elem, oo) in Group(pend, o => o.Element).OrderByDescending(g => g.Value.Count).Select(g => (g.Key, g.Value)))
        {
            var caught = oo.Count(o => o.Cross is ScanCross.AtcCaught or ScanCross.AtcOtherLine);
            coverage[elem] = new JsonObject
            {
                ["pendentes"] = oo.Count,
                ["atc_pegou"] = caught,
                ["mignow"] = oo.Count - caught,
                ["cobertura_atc_pct"] = (double)Math.Round((decimal)(100.0 * caught / oo.Count), 1, MidpointRounding.ToEven),
            };
        }

        var topObjects = MostCommon(mig.Select(o => $"{o.ObjectType} {o.ObjectName}"), 20);
        var topPackages = MostCommon(mig.Select(o => scope.TryGetValue((o.ObjectType, o.ObjectName), out var e) ? e.Package : "?"), 20);
        var objMig = mig.Select(o => (o.ObjectType, o.ObjectName)).Distinct().ToList();

        var readObjects = stats.Sum(s => s.ObjectsRead);
        var objectErrors = stats.Sum(s => s.ObjectErrors);
        var regexErrors = stats.Sum(s => s.RegexErrors);
        var summary = new JsonObject
        {
            ["varredura"] = input.ScanName,
            ["sistema"] = $"{input.Sid}/{input.Client}",
            ["gerado_em"] = input.Now.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
            ["classe_temporaria"] = input.ClassName,
            ["regras_versao"] = input.Rules.Version,
            ["base_atc"] = new JsonObject
            {
                ["arquivo"] = input.AtcBase.File,
                ["serie"] = input.AtcBase.Series,
                ["executada_utc"] = input.AtcBase.ExecutedUtc,
                ["findings"] = input.Findings.Count,
            },
            ["escopo"] = new JsonObject
            {
                ["objetos"] = input.Inventory.Scope,
                ["por_tipo"] = ToObject(input.Inventory.ByType),
                ["excluidos"] = new JsonObject
                {
                    ["tmp"] = input.Inventory.ExcludedTmp,
                    ["marcados_exclusao"] = input.Inventory.ExcludedDeleted,
                    ["gerados"] = input.Inventory.ExcludedGenerated,
                },
            },
            ["lotes"] = input.Batches.Count,
            ["lidos"] = new JsonObject
            {
                ["objetos"] = readObjects,
                ["fontes"] = stats.Sum(s => s.SourcesRead),
                ["linhas"] = stats.Sum(s => s.LinesRead),
                ["erros_objeto"] = objectErrors,
                ["erros_regex"] = regexErrors,
            },
            ["ocorrencias"] = Merge(new JsonObject { ["total"] = occ.Count }, Count(occ.Select(o => Situation(o.Class)))),
            ["cruzamento_pendentes"] = ToObject(Count(pend.Select(o => o.Cross ?? string.Empty))),
            ["mignow"] = new JsonObject
            {
                ["ocorrencias"] = mig.Count,
                ["objetos"] = objMig.Count,
                ["por_tipo_objeto"] = ToObject(Count(objMig.Select(t => t.ObjectType))),
            },
            ["csv_remediacao"] = new JsonObject
            {
                ["arquivo"] = csvName,
                ["linhas"] = toCsv.Count,
                ["prioridade"] = 1,
                ["inclui_outra_linha"] = input.IncludeOtherLine,
                ["por_regra"] = ToObject(Count(toCsv.Select(o => o.RuleId))),
            },
            ["comparacao_original_x_refinada"] = original.Count > 0
                ? new JsonObject
                {
                    ["original_linhas"] = origTotal,
                    ["refinada_ocorrencias"] = occ.Count,
                    ["refinada_pendentes"] = pend.Count,
                    ["refinada_csv"] = toCsv.Count,
                }
                : null,
            ["por_regra"] = porRegra,
            ["cobertura_atc_por_elemento"] = coverage,
            ["top_objetos_mignow"] = Pairs(topObjects),
            ["top_pacotes_mignow"] = Pairs(topPackages),
            ["erros_objeto_amostra"] = Sample(stats.SelectMany(s => s.ObjectErrorSample)),
            ["erros_regex"] = Sample(stats.SelectMany(s => s.RegexErrorSample)),
        };
        return new ScanCrossResult(summary, ScanSummaryMarkdown.Build(summary), csvName, toCsv.Count);
    }

    /// <summary>Escopo por (tipo, nome) a partir dos objetos dos lotes.</summary>
    public static Dictionary<(string Type, string Name), ScanObject> ScopeLookup(IEnumerable<ScanBatch> batches)
    {
        var map = new Dictionary<(string, string), ScanObject>();
        foreach (var o in batches.SelectMany(b => b.Objects))
            map[(o.Type, o.Name)] = o;
        return map;
    }

    /// <summary>Texto da situação como no <c>ocorrencias.csv</c>.</summary>
    public static string Situation(string? flag) => flag switch
    {
        ScanOccurrenceStatus.Pending => "pendente",
        ScanOccurrenceStatus.Remediated => "ja_remediado",
        ScanOccurrenceStatus.Informative => "informativo",
        _ => flag ?? string.Empty,
    };

    /// <summary><c>Counter.most_common()</c>: contagem decrescente, empates na ordem da primeira ocorrência.</summary>
    private static List<KeyValuePair<string, int>> Count(IEnumerable<string> items)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var i in items)
            counts[i] = counts.GetValueOrDefault(i) + 1;
        return [.. counts.OrderByDescending(kv => kv.Value)];
    }

    private static List<KeyValuePair<string, int>> MostCommon(IEnumerable<string> items, int n) => [.. Count(items).Take(n)];

    private static Dictionary<string, List<MignowOccurrence>> Group(IEnumerable<MignowOccurrence> items, Func<MignowOccurrence, string> key)
    {
        var map = new Dictionary<string, List<MignowOccurrence>>(StringComparer.Ordinal);
        foreach (var i in items)
        {
            var k = key(i);
            if (!map.TryGetValue(k, out var l))
                map[k] = l = [];
            l.Add(i);
        }
        return map;
    }

    private static JsonObject ToObject(IEnumerable<KeyValuePair<string, int>> items)
    {
        var o = new JsonObject();
        foreach (var (k, v) in items)
            o[k] = v;
        return o;
    }

    private static JsonObject Merge(JsonObject target, IEnumerable<KeyValuePair<string, int>> items)
    {
        foreach (var (k, v) in items)
            target[k] = v;
        return target;
    }

    private static JsonArray Pairs(IEnumerable<KeyValuePair<string, int>> items) => [.. items.Select(kv => (JsonNode)new JsonArray(kv.Key, kv.Value))];

    private static JsonArray Sample(IEnumerable<IReadOnlyList<string>> rows) =>
        [.. rows.Take(10).Select(r => (JsonNode)new JsonArray([.. r.Select(x => (JsonNode?)JsonValue.Create(x))]))];

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n];
}
