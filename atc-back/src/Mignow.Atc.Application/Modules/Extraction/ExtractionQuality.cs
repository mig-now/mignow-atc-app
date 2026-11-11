namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Dados ausentes no sistema (diferente de zero = dado que o SAP não tem, não erro): sem item, sem TADIR, notas sem título e
/// isentos (<c>exemptionApproval</c> preenchido e diferente de <c>-</c>).
/// </summary>
public sealed record ExtractionQuality(int Exempted, int WithoutItem, int WithoutTadir, int NotesWithoutTitle)
{
    public static ExtractionQuality Of(IReadOnlyList<IReadOnlyDictionary<string, string>> rows)
    {
        string V(IReadOnlyDictionary<string, string> r, string c) => ExtractionTableBuilder.Value(r, c);
        return new ExtractionQuality(
            rows.Count(r => V(r, "exemptionApproval") is { Length: > 0 } a && a != "-"),
            rows.Count(r => V(r, "itm_status").Length == 0),
            rows.Count(r => V(r, "tadir_author").Length == 0),
            rows.Where(r => V(r, "tag_NOTE").Length > 0 && V(r, "note_short_text").Length == 0).Select(r => V(r, "tag_NOTE")).Distinct().Count());
    }
}
