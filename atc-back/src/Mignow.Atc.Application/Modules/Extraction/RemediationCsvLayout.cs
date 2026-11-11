using Mignow.Atc.Application.Modules.Extraction;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Layout do CSV que a atc-remediacao importa (porte de <c>converter.py</c>, desenho §6): cabeçalho da exportação manual do
/// SATC + <c>Finding ID</c> (<c>quickfixInfo</c> = markerId da atc-isencao). Só as colunas com origem confirmada são
/// preenchidas; <see cref="EmptyColumns"/> saem vazias.
/// </summary>
public static class RemediationCsvLayout
{
    private static readonly (string Column, Func<IReadOnlyDictionary<string, string>, string>? Source)[] Layout =
    [
        ("Priority", r => V(r, "priority")),
        ("Check Title", r => V(r, "checkTitle")),
        ("Check Message", r => V(r, "messageTitle")),
        ("Object name", r => V(r, "obj_name")),
        ("Obj.", r => V(r, "obj_type")),
        ("Exemption State", null),
        ("Contact", null),
        ("Package", r => V(r, "obj_packageName")),
        // data (UTC) da criação do item ATC: SATC_AC_ITM-CREATED_ON
        ("1st Found", r => DateBr(V(r, "itm_created_on_utc"))),
        ("Obj. Resp.", r => V(r, "tadir_author")),
        ("Changed by", null),
        ("Note", r => V(r, "tag_NOTE").TrimStart('0')),
        ("Short Text", r => V(r, "note_short_text")),
        ("Component", null),
        ("RObT", r => V(r, "tag_REF_OBJ_TYPE")),
        ("Referenced Object", r => V(r, "tag_REF_OBJ_NAME")),
        ("Additional Info", r => V(r, "tag_ADD_INFO")),
        ("Category", r => V(r, "tag_SITEM_STATE")),
        ("Category7", r => V(r, "tag_CHANGE_CATEGORY")),
        ("Text", r => V(r, "tag_CHANGE_CATEGORY").Length > 0 ? V(r, "change_category_text") : string.Empty),
        ("Remarks", null),
        // markerId do finding (ADT quickfixInfo), usado pela atc-isencao para pedir a isenção do finding exato
        ("Finding ID", r => V(r, "quickfixInfo")),
    ];

    public static IReadOnlyList<string> Header { get; } = [.. Layout.Select(l => l.Column)];

    public static IReadOnlyList<string> EmptyColumns { get; } = [.. Layout.Where(l => l.Source is null).Select(l => l.Column)];

    public static IReadOnlyList<string> Row(IReadOnlyDictionary<string, string> row) => [.. Layout.Select(l => l.Source is null ? string.Empty : l.Source(row))];

    /// <summary>Data <c>DD.MM.AAAA</c> de um timestamp <c>AAAAMMDD…</c>; vazio se não começar com 8 dígitos.</summary>
    public static string DateBr(string ts)
    {
        ts = ts.Trim();
        return ts.Length >= 8 && ts[..8].All(char.IsDigit) ? $"{ts[6..8]}.{ts[4..6]}.{ts[0..4]}" : string.Empty;
    }

    public static byte[] Write(IEnumerable<IReadOnlyDictionary<string, string>> newRows) => ExtractionCsv.WriteMinimal(Header, newRows.Select(Row));

    private static string V(IReadOnlyDictionary<string, string> r, string c) => ExtractionTableBuilder.Value(r, c);
}
