namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Porte de <c>difflib.unified_diff(a, b, fromfile, tofile, lineterm="", n)</c> (sem datas), como o <c>cmd_preparar</c> chama.</summary>
public static class UnifiedDiff
{
    public static List<string> Compute(IReadOnlyList<string> a, IReadOnlyList<string> b, string fromFile, string toFile, int n = 2)
    {
        List<string> result = [];
        var started = false;
        foreach (var group in new SequenceMatcher(a, b).GetGroupedOpcodes(n))
        {
            if (!started)
            {
                started = true;
                result.Add($"--- {fromFile}");
                result.Add($"+++ {toFile}");
            }
            var first = group[0];
            var last = group[^1];
            result.Add($"@@ -{Range(first.I1, last.I2)} +{Range(first.J1, last.J2)} @@");
            foreach (var op in group)
            {
                if (op.Tag == "equal")
                {
                    result.AddRange(a.Skip(op.I1).Take(op.I2 - op.I1).Select(l => " " + l));
                    continue;
                }
                if (op.Tag is "replace" or "delete")
                    result.AddRange(a.Skip(op.I1).Take(op.I2 - op.I1).Select(l => "-" + l));
                if (op.Tag is "replace" or "insert")
                    result.AddRange(b.Skip(op.J1).Take(op.J2 - op.J1).Select(l => "+" + l));
            }
        }
        return result;
    }

    private static string Range(int start, int stop)
    {
        var beginning = start + 1;
        var length = stop - start;
        if (length == 1)
            return beginning.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (length == 0)
            beginning--;
        return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{beginning},{length}");
    }
}
