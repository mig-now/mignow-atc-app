namespace Mignow.Atc.Application.Engine.Diff;

/// <summary>Diferença por linhas (LCS no miolo, depois de tirar o início e o fim iguais), em blocos com contexto.</summary>
public static class LineDiff
{
    public sealed record Hunk(int BeforeStart, IReadOnlyList<string> Before, int AfterStart, IReadOnlyList<string> After,
        IReadOnlyList<string> ContextBefore, IReadOnlyList<string> ContextAfter);

    private const int MaxCells = 4_000_000;

    public static IReadOnlyList<Hunk> Compute(string before, string after, int context = 2)
    {
        var a = Split(before);
        var b = Split(after);
        var prefix = 0;
        while (prefix < a.Length && prefix < b.Length && a[prefix] == b[prefix])
            prefix++;
        var suffix = 0;
        while (suffix < a.Length - prefix && suffix < b.Length - prefix && a[^(suffix + 1)] == b[^(suffix + 1)])
            suffix++;
        var midA = a[prefix..(a.Length - suffix)];
        var midB = b[prefix..(b.Length - suffix)];
        if (midA.Length == 0 && midB.Length == 0)
            return [];

        // Operações: (antes, depois) — índices no miolo; -1 = sem par.
        var ops = (long)midA.Length * midB.Length > MaxCells ? WholeBlock(midA.Length, midB.Length) : Lcs(midA, midB);
        var hunks = new List<Hunk>();
        var i = 0;
        while (i < ops.Count)
        {
            if (ops[i].A >= 0 && ops[i].B >= 0)
            {
                i++;
                continue;
            }
            var start = i;
            while (i < ops.Count && !(ops[i].A >= 0 && ops[i].B >= 0))
                i++;
            var block = ops.GetRange(start, i - start);
            var removed = block.Where(o => o.A >= 0).Select(o => midA[o.A]).ToList();
            var added = block.Where(o => o.B >= 0).Select(o => midB[o.B]).ToList();
            var firstA = prefix + (block.FirstOrDefault(o => o.A >= 0).A is var fa && block.Any(o => o.A >= 0) ? fa : PreviousA(ops, start) + 1);
            var firstB = prefix + (block.Any(o => o.B >= 0) ? block.First(o => o.B >= 0).B : PreviousB(ops, start) + 1);
            var ctxBefore = a[Math.Max(0, firstA - context)..firstA];
            var endA = firstA + removed.Count;
            var ctxAfter = a[endA..Math.Min(a.Length, endA + context)];
            hunks.Add(new Hunk(firstA + 1, removed, firstB + 1, added, ctxBefore, ctxAfter));
        }
        return hunks;
    }

    /// <summary>Trecho antes/depois para a base de conhecimento: os blocos alterados com um pouco de contexto.</summary>
    public static (string Before, string After) Snippets(string before, string after, int context = 1)
    {
        var hunks = Compute(before, after, context);
        var b = string.Join("\n...\n", hunks.Select(h => string.Join('\n', h.ContextBefore.Concat(h.Before).Concat(h.ContextAfter))));
        var a = string.Join("\n...\n", hunks.Select(h => string.Join('\n', h.ContextBefore.Concat(h.After).Concat(h.ContextAfter))));
        return (b, a);
    }

    private static string[] Split(string text) => text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

    private static int PreviousA(List<(int A, int B)> ops, int index)
    {
        for (var j = index - 1; j >= 0; j--)
            if (ops[j].A >= 0)
                return ops[j].A;
        return -1;
    }

    private static int PreviousB(List<(int A, int B)> ops, int index)
    {
        for (var j = index - 1; j >= 0; j--)
            if (ops[j].B >= 0)
                return ops[j].B;
        return -1;
    }

    private static List<(int A, int B)> WholeBlock(int n, int m) =>
        [.. Enumerable.Range(0, n).Select(x => (x, -1)), .. Enumerable.Range(0, m).Select(y => (-1, y))];

    private static List<(int A, int B)> Lcs(string[] a, string[] b)
    {
        var n = a.Length;
        var m = b.Length;
        var dp = new int[n + 1, m + 1];
        for (var x = n - 1; x >= 0; x--)
            for (var y = m - 1; y >= 0; y--)
                dp[x, y] = a[x] == b[y] ? dp[x + 1, y + 1] + 1 : Math.Max(dp[x + 1, y], dp[x, y + 1]);
        var ops = new List<(int, int)>();
        int i = 0, j = 0;
        while (i < n && j < m)
        {
            if (a[i] == b[j])
                ops.Add((i++, j++));
            else if (dp[i + 1, j] >= dp[i, j + 1])
                ops.Add((i++, -1));
            else
                ops.Add((-1, j++));
        }
        while (i < n)
            ops.Add((i++, -1));
        while (j < m)
            ops.Add((-1, j++));
        return ops;
    }
}
