using System.Text;
using System.Text.Json;
using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Application.Modules.Knowledge;

/// <summary>
/// <c>contribuicoes/substituicoes.json</c>: {id_kb: {por, elemento, data, motivo}}, mantido em ordem de inserção e
/// serializado como o <c>json.dumps(..., ensure_ascii=False, indent=1)</c> do Python (formato idêntico ao das skills).
/// </summary>
public sealed class KbSubstitutions
{
    private readonly List<(string Key, List<(string Prop, string Value)> Fields)> _entries = [];

    public static KbSubstitutions Parse(string? json)
    {
        var result = new KbSubstitutions();
        if (string.IsNullOrWhiteSpace(json))
            return result;
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return result;
        foreach (var e in doc.RootElement.EnumerateObject())
        {
            if (e.Value.ValueKind != JsonValueKind.Object)
                continue;
            result._entries.Add((e.Name, [.. e.Value.EnumerateObject().Select(p => (p.Name, p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString()! : p.Value.GetRawText()))]));
        }
        return result;
    }

    public IReadOnlyList<KbReplacement> ToReplacements() =>
        [.. _entries.Select(e => new KbReplacement(e.Key, Get(e.Fields, "por"), Get(e.Fields, "elemento"), Get(e.Fields, "data"),
            Get(e.Fields, "motivo") is { Length: > 0 } m ? m : null))];

    public KbReplacement? Find(string cardId) => ToReplacements().FirstOrDefault(r => r.CardId == cardId);

    /// <summary>Mapa por id (leitura rápida no lookup).</summary>
    public Dictionary<string, KbReplacement> ToMap() => ToReplacements().ToDictionary(r => r.CardId, StringComparer.Ordinal);

    /// <summary>Como <c>st[s] = {...}</c>: atualiza no lugar se já existe, senão acrescenta ao fim.</summary>
    public void Set(string cardId, string by, string element, string date, string reason)
    {
        List<(string, string)> fields = [("por", by), ("elemento", element), ("data", date), ("motivo", reason)];
        var i = _entries.FindIndex(e => e.Key == cardId);
        if (i >= 0)
            _entries[i] = (cardId, fields);
        else
            _entries.Add((cardId, fields));
    }

    public string Serialize()
    {
        if (_entries.Count == 0)
            return "{}";
        var sb = new StringBuilder("{\n");
        for (var i = 0; i < _entries.Count; i++)
        {
            var (key, fields) = _entries[i];
            sb.Append(' ').Append(Quote(key)).Append(": ");
            if (fields.Count == 0)
                sb.Append("{}");
            else
            {
                sb.Append("{\n");
                for (var j = 0; j < fields.Count; j++)
                    sb.Append("  ").Append(Quote(fields[j].Prop)).Append(": ").Append(Quote(fields[j].Value)).Append(j < fields.Count - 1 ? ",\n" : "\n");
                sb.Append(" }");
            }
            sb.Append(i < _entries.Count - 1 ? ",\n" : "\n");
        }
        return sb.Append('}').ToString();
    }

    private static string Get(List<(string Prop, string Value)> fields, string prop) => fields.FirstOrDefault(f => f.Prop == prop).Value ?? "";

    /// <summary>Escape do <c>json.dumps(ensure_ascii=False)</c>: aspas, barra, controles; o resto (inclusive acentos) vai literal.</summary>
    private static string Quote(string s)
    {
        var sb = new StringBuilder("\"");
        foreach (var c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case < ' ': sb.Append("\\u").Append(((int)c).ToString("x4")); break;
                default: sb.Append(c); break;
            }
        }
        return sb.Append('"').ToString();
    }
}
