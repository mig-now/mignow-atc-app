namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>Contagens da extração: total, em baseline e P1/P2/P3 no total e sem baseline ("novos").</summary>
public sealed record ExtractionCounts(int Total, int InBaseline, int P1Total, int P2Total, int P3Total, int P1New, int P2New, int P3New)
{
    public int New => Total - InBaseline;

    public static ExtractionCounts Of(IReadOnlyList<IReadOnlyDictionary<string, string>> rows)
    {
        int[] total = new int[4], novos = new int[4];
        var inBaseline = 0;
        foreach (var r in rows)
        {
            var isNew = ExtractionTableBuilder.IsNew(r);
            if (!isNew)
                inBaseline++;
            if (int.TryParse(ExtractionTableBuilder.Value(r, "priority"), out var p) && p is >= 1 and <= 3)
            {
                total[p]++;
                if (isNew)
                    novos[p]++;
            }
        }
        return new ExtractionCounts(rows.Count, inBaseline, total[1], total[2], total[3], novos[1], novos[2], novos[3]);
    }
}
