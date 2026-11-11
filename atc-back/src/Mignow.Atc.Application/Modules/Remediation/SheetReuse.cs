using System.Text.RegularExpressions;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Porte de <c>cmd_reaproveitar</c>: reaproveita fichas confirmadas de outra sessão do MESMO sistema (mesmo objeto + objeto referenciado). Lê o
/// código atual e o corrigido da ficha de origem, confere se o código atual ainda está no fonte lido agora e monta o bloco com marcadores desta
/// sessão. Não altera status: o consultor decide e registra a decisão (obs citando a ficha de origem).
/// </summary>
public static partial class SheetReuse
{
    public static ReuseResult Reuse(
        IReadOnlyList<RemediationItem> targets, IReadOnlyList<RemediationItem> origin, IReadOnlyCollection<int>? ns, SourceIndex sources, string author,
        DateOnly today)
    {
        var results = new List<ReuseCandidate>();
        foreach (var it in targets)
        {
            if (ns is { Count: > 0 } ? !ns.Contains(it.N) : it.Status.Length > 0)
                continue;
            var reference = it.RefObject ?? string.Empty;
            var r = (N: it.N, Obj: it.ObjectName, Ref: reference);
            var candidates = origin
                .Where(o => o.ObjectName == it.ObjectName && (o.RefObject ?? string.Empty) == reference
                            && o.Status.StartsWith("Confirmado", StringComparison.Ordinal) && !string.IsNullOrEmpty(o.CorrectionSheetMd))
                .OrderBy(o => o.N != it.N).ToList();
            if (candidates.Count == 0)
            {
                results.Add(new ReuseCandidate(r.N, r.Obj, r.Ref, "sem_ficha_na_origem"));
                continue;
            }
            var o = candidates[0];
            var blocks = BlockRegex().Matches(o.CorrectionSheetMd!).Select(m => m.Groups[1].Value).ToList();
            if (blocks.Count < 2)
            {
                results.Add(new ReuseCandidate(r.N, r.Obj, r.Ref, "ficha_sem_codigo", o.N));
                continue;
            }
            var before = SourceIndex.SplitLines(blocks[0].TrimEnd('\n')).ToList();
            if (before.Count > 0 && HeaderRegex().IsMatch(before[0]))
                before.RemoveAt(0);
            var corrected = SourceIndex.SplitLines(blocks[1].TrimEnd('\n'));
            var start = Enumerable.Range(0, corrected.Length).Where(i => InsertionStart().IsMatch(corrected[i])).ToList();
            var end = Enumerable.Range(0, corrected.Length).Where(i => InsertionEnd().IsMatch(corrected[i])).ToList();
            var deletions = corrected.Count(l => DeletionStart().IsMatch(l));
            if (start.Count != 1 || end.Count != 1 || deletions > 1)
            {
                results.Add(new ReuseCandidate(r.N, r.Obj, r.Ref, "varios_blocos_revisar", o.N));
                continue;
            }
            var after = corrected[(start[0] + 1)..end[0]].ToList();
            var hits = Occurrences(sources, it.ObjectName, before);
            if (hits.Count == 0)
            {
                results.Add(new ReuseCandidate(r.N, r.Obj, r.Ref, "codigo_atual_mudou", o.N));
                continue;
            }
            List<string> block =
            [
                Marker(author, today, "START", "DELETION"), .. before.Select(l => "*" + l), Marker(author, today, "END", "DELETION"),
                Marker(author, today, "START", "INSERTION"), .. after, Marker(author, today, "END", "INSERTION"),
            ];
            results.Add(new ReuseCandidate(r.N, r.Obj, r.Ref, hits.Count == 1 ? "ok" : "ok_trecho_repetido_use_linha", o.N, o.Status,
                o.Obs.Length > 160 ? o.Obs[..160] : o.Obs, hits, before.Count, after.Count, before, after, block));
        }
        var summary = results.GroupBy(x => x.Situation).ToDictionary(g => g.Key, g => g.Count());
        return new ReuseResult(summary, results);
    }

    private static List<ReuseOccurrence> Occurrences(SourceIndex sources, string objectName, List<string> before)
    {
        var hits = new List<ReuseOccurrence>();
        var needle = before.Select(l => l.TrimEnd()).ToList();
        foreach (var (name, lines) in sources.Files(objectName))
        {
            var text = lines.Select(l => l.TrimEnd()).ToList();
            for (var i = 0; i <= text.Count - needle.Count; i++)
            {
                if (text.Skip(i).Take(needle.Count).SequenceEqual(needle))
                    hits.Add(new ReuseOccurrence(name, i + 1));
            }
        }
        return hits;
    }

    private static string Marker(string author, DateOnly today, string startEnd, string kind) =>
        $"*{(startEnd == "START" ? ">>>" : "<<<")} S/4 MIGRATION MIG-NOW - {startEnd} OF {kind} - {today:dd.MM.yyyy} {author}";

    [GeneratedRegex("```abap\n(.*?)```", RegexOptions.Singleline)]
    private static partial Regex BlockRegex();

    [GeneratedRegex(@"^\*\s*\S+\s+-\s+linhas?\s+\d")]
    private static partial Regex HeaderRegex();

    [GeneratedRegex(@"START\s+OF\s+INSERTION")]
    private static partial Regex InsertionStart();

    [GeneratedRegex(@"END\s+OF\s+INSERTION")]
    private static partial Regex InsertionEnd();

    [GeneratedRegex(@"START\s+OF\s+DELETION")]
    private static partial Regex DeletionStart();
}
