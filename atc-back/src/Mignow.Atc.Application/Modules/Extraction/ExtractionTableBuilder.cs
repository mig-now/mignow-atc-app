using Mignow.Atc.Application.Ports.Sap.Extraction;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Junta os findings do ADT com os complementos do SQL e fixa a ordem das colunas (porte das seções 3 e 4 do
/// <c>extrair.ps1</c>): objeto, TADIR, finding, <c>item_id</c>/<c>documentation_link</c>/<c>exemption_kind_text</c>, tags,
/// <c>change_category_text</c>, <c>note_short_text</c>, item ATC (<c>itm_*</c>) e baseline.
/// </summary>
public static class ExtractionTableBuilder
{
    public const string BaselineEffect = "baseline_effect";

    public static IReadOnlyList<string> ComplementKeys(AtcRawFindings raw, string key) =>
        [.. raw.Rows.Select(r => r.GetValueOrDefault(key) ?? string.Empty).Where(v => v.Length > 0)];

    public static ExtractionTable Build(AtcRawFindings raw, AtcComplements c)
    {
        var rows = new List<IReadOnlyDictionary<string, string>>(raw.Rows.Count);
        foreach (var source in raw.Rows)
        {
            var l = new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase);
            var itemId = l.GetValueOrDefault("item_id") ?? string.Empty;
            if (c.Items.TryGetValue(itemId, out var i))
            {
                l["itm_type"] = i.Type;
                l["itm_type_text"] = Text(c.TypeTexts, i.Type);
                l["itm_created_on_utc"] = i.CreatedOn;
                l["itm_updated_on_utc"] = i.UpdatedOn;
                l["itm_last_run_index"] = i.LastRunIndex;
                l["itm_processor"] = i.Processor;
                l["itm_status"] = i.Status;
                l["itm_status_text"] = Text(c.StatusTexts, i.Status);
            }
            if (c.Baselines.TryGetValue(itemId, out var b) && b.Count > 0)
            {
                l["baseline_ix"] = string.Join(',', b.Select(x => x.BaselineIx));
                l[BaselineEffect] = string.Join(',', SortedUnique(b.Select(x => x.BaselineEffect)));
                l["baseline_effect_text"] = string.Join(", ", SortedUnique(b.Select(x => Text(c.EffectTexts, x.BaselineEffect))));
            }
            if (c.Tadir.TryGetValue($"{l.GetValueOrDefault("obj_type")}|{l.GetValueOrDefault("obj_name")}", out var t))
            {
                l["tadir_author"] = t.Author;
                l["tadir_devclass"] = t.DevClass;
            }
            var note = l.GetValueOrDefault("tag_NOTE") ?? string.Empty;
            if (note.Length > 0)
                l["note_short_text"] = Text(c.NoteTitles, note);
            if (l.TryGetValue("tag_CHANGE_CATEGORY", out var category))
                l["change_category_text"] = Text(c.CategoryTexts, category);
            if (l.TryGetValue("exemptionKind", out var kind))
                l["exemption_kind_text"] = Text(c.ExemptionTexts, kind);
            rows.Add(l);
        }
        return new ExtractionTable(Columns(raw), rows);
    }

    /// <summary>Ordem das colunas do CSV total (igual ao <c>$colunas</c> do script, sem repetir nomes).</summary>
    public static IReadOnlyList<string> Columns(AtcRawFindings raw) =>
        [.. raw.ObjectColumns
            .Concat(["tadir_author", "tadir_devclass"])
            .Concat(raw.FindingColumns)
            .Concat(["item_id", "documentation_link", "exemption_kind_text"])
            .Concat(raw.TagColumns)
            .Concat(["change_category_text", "note_short_text", "itm_type", "itm_type_text", "itm_created_on_utc", "itm_updated_on_utc",
                "itm_last_run_index", "itm_processor", "itm_status", "itm_status_text", "baseline_ix", BaselineEffect, "baseline_effect_text"])
            .Distinct(StringComparer.Ordinal)];

    /// <summary>Sem entrada de baseline (<c>SATC_AC_BSI</c>) = "novo": só estes vão para a remediação.</summary>
    public static bool IsNew(IReadOnlyDictionary<string, string> row) => string.IsNullOrEmpty(Value(row, BaselineEffect));

    public static string Value(IReadOnlyDictionary<string, string> row, string column) => row.TryGetValue(column, out var v) ? v : string.Empty;

    private static string Text(IReadOnlyDictionary<string, string> texts, string key) => texts.TryGetValue(key, out var v) ? v : string.Empty;

    /// <summary><c>Sort-Object -Unique</c> do PowerShell: sem diferenciar maiúsculas, cultura invariante.</summary>
    private static IEnumerable<string> SortedUnique(IEnumerable<string> values) =>
        values.Distinct(StringComparer.InvariantCultureIgnoreCase).Order(StringComparer.InvariantCultureIgnoreCase);
}
