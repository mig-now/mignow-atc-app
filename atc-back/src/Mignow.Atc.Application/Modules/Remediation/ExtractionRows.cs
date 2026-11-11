using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Domain.Extraction;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Findings de uma extração no layout do CSV que a remediação importa (porte do <c>LAYOUT</c> do <c>converter.py</c> da atc-extracao): as colunas
/// vêm do <c>AllColumnsJson</c> (objeto nome → valor, nomes do CSV total) e, na falta, dos campos indexados do finding. Colunas sem origem
/// confirmada saem vazias.
/// </summary>
public static class ExtractionRows
{
    public static readonly IReadOnlyList<string> Layout =
    [
        "Priority", "Check Title", "Check Message", "Object name", "Obj.", "Exemption State", "Contact", "Package", "1st Found", "Obj. Resp.", "Changed by",
        "Note", "Short Text", "Component", "RObT", "Referenced Object", "Additional Info", "Category", "Category7", "Text", "Remarks", "Finding ID",
    ];

    public static CsvDocument ToDocument(IEnumerable<AtcFinding> findings) =>
        new(AtcCsv.Utf8Sig, ";", Layout, [.. findings.OrderBy(f => f.Seq).Select(Row)]);

    public static IReadOnlyList<string> Row(AtcFinding f)
    {
        var all = ExtractionFiles.ParseRow(f.AllColumnsJson);
        string G(string key, string? fallback = null) => all.TryGetValue(key, out var v) ? v : fallback ?? string.Empty;
        var category7 = G("tag_CHANGE_CATEGORY");
        return
        [
            G("priority", f.Priority?.ToString()), G("checkTitle", f.CheckTitle), G("messageTitle", f.MessageTitle), G("obj_name", f.ObjectName),
            G("obj_type", f.ObjectType), string.Empty, string.Empty, G("obj_packageName", f.Package), DateBr(G("itm_created_on_utc")), G("tadir_author"),
            string.Empty, G("tag_NOTE", f.Note).TrimStart('0'), G("note_short_text"), string.Empty, G("tag_REF_OBJ_TYPE", f.RefObjectType),
            G("tag_REF_OBJ_NAME", f.RefObjectName), G("tag_ADD_INFO"), G("tag_SITEM_STATE"), category7, category7.Length > 0 ? G("change_category_text") : string.Empty,
            string.Empty, G("quickfixInfo", f.FindingId),
        ];
    }

    private static string DateBr(string ts)
    {
        ts = ts.Trim();
        return ts.Length >= 8 && ts[..8].All(char.IsAsciiDigit) ? $"{ts[6..8]}.{ts[4..6]}.{ts[..4]}" : string.Empty;
    }
}
