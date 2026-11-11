using System.Globalization;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>Linha H da saída: ocorrência (flag P/R/I; <see cref="Snippet"/> = o resto da linha unido por tab).</summary>
public sealed record ScanHit(string Type, string Name, string Source, int Line, int LineEnd, string RuleId, string Element, string Flag, string Snippet);

/// <summary>Saída de um lote já separada (<c>ler_saidas</c> do <c>cruzar.py</c>): H, F, E, O, X e o Z de fim de lote.</summary>
public sealed record ScanOutput(
    IReadOnlyList<ScanHit> Hits, IReadOnlyList<IReadOnlyList<string>> Read, IReadOnlyList<IReadOnlyList<string>> ObjectErrors,
    IReadOnlyList<IReadOnlyList<string>> Original, IReadOnlyList<IReadOnlyList<string>> RegexErrors, int? EndedLot, int ServerMs, int ScanErrors)
{
    /// <summary>O lote terminou (linha Z)? Sem ela a execução foi interrompida.</summary>
    public bool Complete => EndedLot is not null;

    /// <summary>Contagens e amostras (10 primeiras de E e X) guardadas no lote.</summary>
    public ScanBatchStats ToStats(int? timeSeconds, string? message, IReadOnlyList<string> messages, DateTimeOffset? startedAt)
    {
        var original = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var o in Original)
            if (o.Count >= 5 && int.TryParse(o[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var q))
                original[o[3]] = original.GetValueOrDefault(o[3]) + q;
        return new ScanBatchStats(
            Read.Count, Read.Sum(x => Int(x, 2)), Read.Sum(x => Int(x, 3)), Hits.Count, ObjectErrors.Count, RegexErrors.Count, ServerMs, ScanErrors,
            timeSeconds, message, messages, [.. ObjectErrors.Take(10)], [.. RegexErrors.Take(10)], original, startedAt);
    }

    private static int Int(IReadOnlyList<string> x, int i) => x.Count > i && int.TryParse(x[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
}

/// <summary>Parse da saída do classrun (uma linha por registro, campos separados por tab): H, F, E, O, X, Z (e L, ignorada).</summary>
public static class ScanOutputParser
{
    public static ScanOutput Parse(string output)
    {
        var hits = new List<ScanHit>();
        List<IReadOnlyList<string>> read = [], errors = [], original = [], regex = [];
        int? ended = null;
        var serverMs = 0;
        var scanErrors = 0;
        foreach (var line in output.Split(["\r\n", "\n", "\r"], StringSplitOptions.None))
        {
            var c = line.Split('\t');
            switch (c[0])
            {
                case "Z" when c.Length >= 2:
                    if (int.TryParse(c[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var lot))
                        ended = lot;
                    if (c.Length >= 5)
                    {
                        _ = int.TryParse(c[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out serverMs);
                        _ = int.TryParse(c[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out scanErrors);
                    }
                    break;
                case "H" when c.Length >= 9:
                    hits.Add(new ScanHit(c[1], c[2], c[3], I(c[4]), I(c[5]), c[6], c[7], c[8], string.Join('\t', c.Skip(9))));
                    break;
                case "F":
                    read.Add([.. c.Skip(1)]);
                    break;
                case "E":
                    errors.Add([.. c.Skip(1)]);
                    break;
                case "O":
                    original.Add([.. c.Skip(1)]);
                    break;
                case "X":
                    regex.Add([.. c.Skip(1)]);
                    break;
            }
        }
        return new ScanOutput(hits, read, errors, original, regex, ended, serverMs, scanErrors);
    }

    private static int I(string s) => int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
}
