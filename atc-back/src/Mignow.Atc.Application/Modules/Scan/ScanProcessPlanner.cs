using System.Globalization;
using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>Seleção de lotes (<c>1</c>, <c>2-9</c>, <c>1,3</c>), rodízio entre processos e nomes das classes (<c>&lt;base&gt;_1.._N</c>) do <c>executar_paralelo.ps1</c>.</summary>
public static partial class ScanProcessPlanner
{
    public const int MaxParallel = 9;

    public static IReadOnlyList<int> ParseLots(string? selection, IReadOnlyCollection<int> all)
    {
        if (string.IsNullOrWhiteSpace(selection))
            return [.. all.Order()];
        var wanted = new List<int>();
        foreach (var part in selection.Split(','))
        {
            var m = RangeRegex().Match(part);
            if (m.Success)
            {
                var a = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                var b = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                for (var i = a; i <= b; i++)
                    wanted.Add(i);
            }
            else if (int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                wanted.Add(n);
        }
        return [.. wanted.Where(all.Contains).Distinct().Order()];
    }

    /// <summary>Repartição em rodízio: o k-ésimo lote vai ao processo (k mod n) + 1; n = min(paralelo, lotes).</summary>
    public static IReadOnlyList<IReadOnlyList<int>> RoundRobin(IReadOnlyList<int> lots, int parallel)
    {
        if (lots.Count == 0)
            return [];
        var n = Math.Min(Math.Clamp(parallel, 1, MaxParallel), lots.Count);
        var groups = Enumerable.Range(0, n).Select(_ => new List<int>()).ToList();
        for (var k = 0; k < lots.Count; k++)
            groups[k % n].Add(lots[k]);
        return groups;
    }

    public static string ProcessClass(string baseClass, int process) => $"{baseClass}_{process}";

    public static IReadOnlyList<string> AllClasses(string baseClass, int parallel) =>
        [baseClass, .. Enumerable.Range(1, parallel).Select(w => ProcessClass(baseClass, w))];

    public static bool ValidClassName(string name) => ClassRegex().IsMatch(name);

    [GeneratedRegex(@"^\s*(\d+)\s*-\s*(\d+)\s*$")]
    private static partial Regex RangeRegex();

    [GeneratedRegex("^[ZY][A-Z0-9_]{1,29}$")]
    private static partial Regex ClassRegex();
}
