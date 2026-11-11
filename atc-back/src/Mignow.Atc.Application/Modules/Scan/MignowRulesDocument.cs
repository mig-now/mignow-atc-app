using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// <c>regras.json</c> da skill atc-extracao-mignow: versão, gatilho (pré-filtro) e regras. O texto JSON original é guardado
/// como snapshot da varredura; a seleção de regras ativas (<c>PUT scope</c>) muda só o campo <c>ativa</c> de cada regra.
/// </summary>
public sealed record MignowRulesDocument(string? Version, string? Date, string Trigger, IReadOnlyList<MignowRule> Rules)
{
    public IEnumerable<MignowRule> ActiveRules => Rules.Where(r => r.Active);

    public static MignowRulesDocument Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var rules = new List<MignowRule>();
        foreach (var r in root.GetProperty("regras").EnumerateArray())
        {
            r.TryGetProperty("refinada", out var f);
            r.TryGetProperty("origem", out var o);
            rules.Add(new MignowRule(
                Str(r, "id"), !r.TryGetProperty("ativa", out var a) || a.ValueKind != JsonValueKind.False,
                r.TryGetProperty("prioridade", out var p) ? Raw(p) : "1",
                Str(r, "categoria"), Str(r, "severidade"), Str(r, "descricao"), Str(o, "pcre"), Str(f, "pcre"), Str(f, "texto"),
                Str(f, "elemento_fixo"), Str(f, "exige"), Str(f, "suprimida_por"), Str(r, "robt")));
        }
        return new MignowRulesDocument(root.TryGetProperty("versao", out var v) ? Raw(v) : null,
            root.TryGetProperty("data", out var d) ? Raw(d) : null, Str(root, "gatilho"), rules);
    }

    /// <summary>Marca como ativas só as regras da lista (as demais <c>ativa = false</c>) e devolve o novo JSON.</summary>
    public static string WithActive(string json, IReadOnlyCollection<string> activeIds)
    {
        var node = JsonNode.Parse(json)!.AsObject();
        foreach (var rule in node["regras"]!.AsArray())
        {
            var obj = rule!.AsObject();
            obj["ativa"] = activeIds.Contains(obj["id"]!.GetValue<string>(), StringComparer.Ordinal);
        }
        return node.ToJsonString(new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    }

    private static string Str(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? string.Empty : string.Empty;

    private static string Raw(JsonElement e) => e.ValueKind == JsonValueKind.String ? e.GetString() ?? string.Empty : e.GetRawText();

    internal static string FormatNumber(double n) => n.ToString(CultureInfo.InvariantCulture);
}
