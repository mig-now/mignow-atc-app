using System.Text.Json;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>Montagem dos três CSVs a partir das linhas gravadas e filtro de prioridades do download.</summary>
public static class ExtractionFiles
{
    /// <summary><c>"1,2"</c> → {1, 2}. Vazio = sem filtro (lista vazia); valor fora de 1..3 = inválido (nulo).</summary>
    public static IReadOnlyList<int>? ParsePriorities(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];
        var parts = text.Split(',', StringSplitOptions.TrimEntries);
        var list = new List<int>();
        foreach (var p in parts)
        {
            if (!int.TryParse(p, out var v) || v is < 1 or > 3)
                return null;
            if (!list.Contains(v))
                list.Add(v);
        }
        return list.Order().ToList();
    }

    /// <summary>Nome da skill; com filtro de prioridades acrescenta <c>_p1-2</c> antes da extensão (sem filtro = nome idêntico ao da skill).</summary>
    public static string FileName(string baseName, ExtractionFileKind kind, IReadOnlyList<int> priorities)
    {
        var name = ExtractionNaming.FileName(baseName, kind);
        if (priorities.Count == 0)
            return name;
        var dot = name.LastIndexOf('.');
        return $"{name[..dot]}_p{string.Join('-', priorities)}{name[dot..]}";
    }

    public static byte[] Content(ExtractionFileKind kind, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyDictionary<string, string>> rows) =>
        kind == ExtractionFileKind.Remediation ? RemediationCsvLayout.Write(rows) : ExtractionCsv.WriteQuotedAll(columns, rows);

    /// <summary>
    /// Formato único de <c>AtcFinding.AllColumnsJson</c> (gravado pelo passo de extração): objeto JSON nome da coluna do CSV total → valor
    /// (texto). Todos os leitores (remediação, isenção, varredura, relatório, exportação) passam por aqui. Chaves sem diferenciar maiúsculas;
    /// JSON ilegível ou que não é objeto = sem colunas.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ParseRow(string json)
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return row;
            foreach (var p in doc.RootElement.EnumerateObject())
                row[p.Name] = p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? string.Empty : p.Value.ToString();
        }
        catch (JsonException)
        {
        }
        return row;
    }
}
