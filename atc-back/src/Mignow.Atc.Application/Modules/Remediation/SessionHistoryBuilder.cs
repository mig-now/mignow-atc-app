using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Cálculo do histórico (<see cref="SessionHistory"/>), puro e testável.</summary>
public static class SessionHistoryBuilder
{
    /// <summary>Pausa que deixa de contar no tempo líquido (<c>tempo_liquido</c> da skill atc-extracao).</summary>
    public static readonly TimeSpan IdleGap = TimeSpan.FromMinutes(10);

    public static readonly IReadOnlyList<string> Categories = ["KB", "IA", "Isenção", "Cancelado", "Pendente"];

    /// <summary><c>cat()</c> do resultado.py.</summary>
    public static string Category(string? status)
    {
        var s = status ?? string.Empty;
        return s.Contains("KB", StringComparison.Ordinal) ? "KB"
            : s.Contains("IA", StringComparison.Ordinal) ? "IA"
            : s.StartsWith("Cancel", StringComparison.Ordinal) ? "Cancelado"
            : s.StartsWith("Isen", StringComparison.Ordinal) ? "Isenção"
            : "Pendente";
    }

    public static SessionHistory Build(DateTimeOffset sessionStart, IReadOnlyList<HistoryItem> items, IReadOnlyList<HistoryRun> runs)
    {
        var before = items.ToDictionary(i => i.N, i => StatusAt(i, sessionStart));
        var after = items.ToDictionary(i => i.N, i => i.Status);
        var transitions = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var handled = new List<int>();
        foreach (var i in items)
        {
            if (Category(before[i.N]) == Category(after[i.N]))
                continue;
            var key = $"{Category(before[i.N])} → {Category(after[i.N])}";
            transitions[key] = transitions.GetValueOrDefault(key) + 1;
            handled.Add(i.N);
        }
        // Linha do tempo: o fim de cada dia em que algum item mudou de status.
        var days = items.SelectMany(i => i.History).Select(h => DateOnly.FromDateTime(h.At.UtcDateTime)).Distinct().Order().ToList();
        var timeline = days.Select(d =>
        {
            var end = new DateTimeOffset(d.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddTicks(-1);
            return new TimelinePoint(d, Count(items, i => StatusAt(i, end)));
        }).ToList();
        var activity = runs.Where(r => r.StartedAt is not null)
            .Select(r => (Start: r.StartedAt!.Value, End: r.FinishedAt ?? r.StartedAt!.Value))
            .Concat(items.SelectMany(i => i.History).Where(h => h.At >= sessionStart).Select(h => (Start: h.At, End: h.At)))
            .OrderBy(a => a.Start).ToList();
        return new SessionHistory(Count(items, i => before[i.N]), Count(items, i => after[i.N]), handled.Count, transitions, [.. handled.Order()], timeline,
            Wall(activity), Net(activity), runs, runs.Sum(r => r.AiInputTokens), runs.Sum(r => r.AiOutputTokens), runs.Sum(r => r.AiCost), runs.Sum(r => r.AiCalls));
    }

    /// <summary>Status do item no instante (última entrada do histórico até ele; vazio antes da primeira).</summary>
    public static string StatusAt(HistoryItem item, DateTimeOffset at) =>
        item.History.Where(h => h.At <= at).OrderBy(h => h.At).LastOrDefault()?.Status ?? string.Empty;

    private static SituationCounts Count(IReadOnlyList<HistoryItem> items, Func<HistoryItem, string> status)
    {
        var withStatus = items.Select(i => (Item: i, Cat: Category(status(i)))).ToList();
        int N(string c) => withStatus.Count(x => x.Cat == c);
        return new SituationCounts(N("KB"), N("IA"), N("Isenção"), N("Cancelado"), N("Pendente"), items.Select(i => i.ObjectKey).Distinct().Count(),
            withStatus.Where(x => x.Cat == "Pendente").Select(x => x.Item.ObjectKey).Distinct().Count());
    }

    private static int Wall(List<(DateTimeOffset Start, DateTimeOffset End)> activity) =>
        activity.Count == 0 ? 0 : (int)(activity.Max(a => a.End) - activity[0].Start).TotalSeconds;

    /// <summary>Soma dos trechos de atividade; pausa acima de <see cref="IdleGap"/> entre dois trechos não conta.</summary>
    private static int Net(List<(DateTimeOffset Start, DateTimeOffset End)> activity)
    {
        if (activity.Count == 0)
            return 0;
        var total = TimeSpan.Zero;
        var (segStart, segEnd) = activity[0];
        foreach (var (start, end) in activity.Skip(1))
        {
            if (start - segEnd <= IdleGap)
                segEnd = end > segEnd ? end : segEnd;
            else
            {
                total += segEnd - segStart;
                (segStart, segEnd) = (start, end);
            }
        }
        total += segEnd - segStart;
        return (int)total.TotalSeconds;
    }
}
