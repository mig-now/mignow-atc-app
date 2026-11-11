using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Mignow.Atc.Application.Ports.Sap.Extraction;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Módulo Extraction: execuções, findings e complementos do ATC gerados do cenário (chamar dentro de <c>WithLock</c>).</summary>
public sealed partial class SimulatedSapState
{
    private static readonly string[] SimTagColumns =
        ["tag_ADD_INFO", "tag_CHANGE_CATEGORY", "tag_NOTE", "tag_REF_OBJ_NAME", "tag_REF_OBJ_TYPE", "tag_SITEM_STATE"];

    private ScenarioAtcRun SimRun => Scenario.AtcRuns.FirstOrDefault() ?? new ScenarioAtcRun("ATC-SIM-001", "ATC (simulado)", new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero));

    public string SimDisplayId => SimRun.Id;

    /// <summary>Cabeçalhos como o SQL do adapter devolveria: principal, uma execução mais antiga, uma reavaliação e (opcional) uma em andamento.</summary>
    public IReadOnlyList<AtcRunHeader> AtcHeaders(IReadOnlyList<string> prefixes)
    {
        var run = SimRun;
        var series = Scenario.AtcExtraction.Series;
        var findings = AtcFindings(run.Id)!.Rows;
        int Count(int p) => findings.Count(r => r["priority"] == p.ToString(CultureInfo.InvariantCulture));
        string Ts(DateTimeOffset d) => d.UtcDateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".0000000";
        var list = new List<AtcRunHeader>
        {
            new(run.Id, run.Id, run.Title, series, Ts(run.CreatedAt), SimulatorUser, true, false, "SIM_VARIANT", "SIM", Count(1), Count(2), Count(3), 0, 0),
            new(run.Id + "-OLD", run.Id + "-OLD", run.Title + " (anterior)", series, Ts(run.CreatedAt.AddDays(-1)), SimulatorUser, true, false, "SIM_VARIANT", "SIM", 0, 0, 0, 0, 0),
            new(run.Id + "-R", run.Id, run.Title, series, Ts(run.CreatedAt.AddMinutes(5)), SimulatorUser, true, true, "SIM_VARIANT", "SIM", 0, 0, 0, 0, 0),
        };
        if (Scenario.AtcExtraction.InProgressRun)
            list.Add(new(run.Id + "-NEW", run.Id + "-NEW", run.Title + " (em andamento)", series, Ts(run.CreatedAt.AddDays(1)), SimulatorUser, false, false,
                "SIM_VARIANT", "SIM", 0, 0, 0, 0, 0));
        return [.. list.Where(h => prefixes.Any(p => LikePrefix(h.Series, p)))];
    }

    private static readonly string[] SqlKeywords = ["FROM", "JOIN", "INTO", "UPDATE", "TABLE"];

    /// <summary>
    /// Objeto referenciado do finding simulado: a última palavra do padrão ("from bseg" → BSEG); quando ela é palavra-chave de SQL
    /// ("select * from") ou ainda o campo de uma variável ("lv_matnr(18)" → MATNR), o objeto real da linha de código (a tabela do SELECT).
    /// </summary>
    private static string ReferencedObject(string pattern, string line)
    {
        var word = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last().Split('(')[0].ToUpperInvariant();
        if (SqlKeywords.Contains(word))
        {
            var at = line.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            var rest = at < 0 ? string.Empty : line[(at + pattern.Length)..];
            var next = rest.Split([' ', '.', ',', '(', ')'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.TrimStart('@');
            if (!string.IsNullOrEmpty(next))
                return next.ToUpperInvariant();
        }
        else if (pattern.Contains('(') && word.StartsWith("LV_", StringComparison.Ordinal))
            return word[3..];
        return word;
    }

    /// <summary>Findings da execução (nulo se o display id não existe): um por linha de código não comentada que casa com uma regra do ATC.</summary>
    public AtcRawFindings? AtcFindings(string displayId)
    {
        var run = SimRun;
        if (!displayId.StartsWith(run.Id, StringComparison.OrdinalIgnoreCase))
            return null;
        var rows = new List<IReadOnlyDictionary<string, string>>();
        foreach (var o in _objects.Values.Where(IsCustomer))
            for (var r = 0; r < Scenario.AtcRules.Count; r++)
            {
                var rule = Scenario.AtcRules[r];
                var lines = o.ActiveSource.Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith('*') || !lines[i].Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var itemId = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes($"{run.Id}|{o.Ref.Name}|{i + 1}|{r}")));
                    var checkId = rule.Code ?? $"SIM_{r + 1:00}";
                    var word = ReferencedObject(rule.Pattern, lines[i]);
                    rows.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["obj_uri"] = o.Ref.Uri, ["obj_type"] = o.Ref.Type.Split('/')[0], ["obj_name"] = o.Ref.Name, ["obj_packageName"] = o.Ref.Package,
                        ["uri"] = $"/sap/bc/adt/atc/results/{displayId}/items/itemid/{itemId}", ["location"] = $"{o.Ref.Uri}/source/main#start={i + 1},0",
                        ["priority"] = rule.Priority.ToString(CultureInfo.InvariantCulture), ["checkId"] = checkId, ["checkTitle"] = rule.Check ?? "ATC",
                        ["messageId"] = checkId, ["messageTitle"] = rule.Message, ["exemptionApproval"] = "-", ["quickfixInfo"] = "MK" + itemId[..12],
                        ["lastChangedBy"] = "DEVELOPER", ["item_id"] = itemId, ["documentation_link"] = $"/sap/bc/adt/docu/atc/{checkId.ToLowerInvariant()}",
                        ["tag_ADD_INFO"] = "simulated", ["tag_CHANGE_CATEGORY"] = "1", ["tag_NOTE"] = (2_000_000 + r).ToString("D10", CultureInfo.InvariantCulture),
                        ["tag_REF_OBJ_NAME"] = word, ["tag_REF_OBJ_TYPE"] = rule.Pattern.Contains('(') ? "DTEL" : "TABL", ["tag_SITEM_STATE"] = "SIM",
                    });
                }
            }
        return new AtcRawFindings(["obj_uri", "obj_type", "obj_name", "obj_packageName"],
            ["uri", "location", "priority", "checkId", "checkTitle", "messageId", "messageTitle", "exemptionApproval", "quickfixInfo", "lastChangedBy"],
            SimTagColumns, rows);
    }

    public AtcComplements AtcComplementsFor(string displayId)
    {
        var rows = AtcFindings(displayId)?.Rows ?? [];
        var items = new Dictionary<string, AtcItem>(StringComparer.OrdinalIgnoreCase);
        var baselines = new Dictionary<string, IReadOnlyList<AtcBaselineEntry>>(StringComparer.OrdinalIgnoreCase);
        var tadir = new Dictionary<string, AtcTadirEntry>(StringComparer.OrdinalIgnoreCase);
        var notes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var missing = Scenario.AtcExtraction.MissingTables ?? [];
        var noBaseline = missing.Contains("SATC_AC_BSI", StringComparer.OrdinalIgnoreCase);
        var noNotes = missing.Contains("CWBNTSTXT", StringComparer.OrdinalIgnoreCase);
        var n = 0;
        foreach (var r in rows)
        {
            n++;
            var id = r["item_id"];
            items[id] = new AtcItem("1", SimRun.CreatedAt.UtcDateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture), string.Empty, "1", string.Empty, "N");
            if (!noBaseline && Scenario.AtcExtraction.BaselineEvery > 0 && n % Scenario.AtcExtraction.BaselineEvery == 0)
                baselines[id] = [new AtcBaselineEntry("0001", "E")];
            tadir[$"{r["obj_type"]}|{r["obj_name"]}"] = new AtcTadirEntry("DEVELOPER", r["obj_packageName"]);
            if (!noNotes)
                notes[r["tag_NOTE"]] = "Simulated SAP note " + r["tag_NOTE"].TrimStart('0');
        }
        return new AtcComplements(items, new Dictionary<string, string> { ["N"] = "New" }, new Dictionary<string, string> { ["1"] = "Finding" }, baselines,
            new Dictionary<string, string> { ["E"] = "Exempted by baseline" }, tadir, notes, new Dictionary<string, string> { ["1"] = "Simplification item" },
            new Dictionary<string, string>(), [.. missing.Select(m => m.ToUpperInvariant())]);
    }

    /// <summary>LIKE do adapter ('_' vira curinga): a série começa com o prefixo.</summary>
    private static bool LikePrefix(string series, string prefix) =>
        series.Length >= prefix.Length && prefix.Select((c, i) => c == '_' || char.ToUpperInvariant(series[i]) == char.ToUpperInvariant(c)).All(x => x);
}
