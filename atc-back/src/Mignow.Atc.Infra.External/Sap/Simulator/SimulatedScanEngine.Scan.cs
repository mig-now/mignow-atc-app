using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Mignow.Atc.Application.Ports.Sap.Scan;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Fonte de um objeto como a classe o lê: caminho ADT (sem <c>/sap/bc/adt/</c>) e linhas.</summary>
internal sealed record SimulatedScanSource(string Uri, IReadOnlyList<string> Lines);

/// <summary>
/// O que a classe temporária faz no SAP, sobre os fontes do cenário: o mesmo formato de saída (L/F/E/H/O/X/Z) e as mesmas regras de
/// decisão do template ABAP (gatilho, sentenças sem comentários, texto L/C, elemento, supressão, exige, remediado). Aproximação: a
/// divisão em sentenças é feita por um tokenizador simples no lugar do <c>SCAN ABAP-SOURCE</c> do kernel, e as regex rodam no .NET (os
/// POSIX <c>[[:space:]]</c> das regras originais são convertidos).
/// </summary>
internal static partial class SimulatedScanEngine
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

    public static string Run(ScanLotRequest lot, Func<ScanLotObject, IReadOnlyList<SimulatedScanSource>> sourcesOf)
    {
        var sw = Stopwatch.StartNew();
        var output = new List<string> { $"L\t{lot.Number:D3}\t{lot.Objects.Count}" };
        var trigger = new Regex("(?i)" + lot.Trigger.Replace("(?i)", string.Empty, StringComparison.Ordinal), RegexOptions.CultureInvariant, RegexTimeout);
        var broken = new HashSet<string>(StringComparer.Ordinal);
        var rules = lot.Rules.Select(r => (Spec: r, Rx: Compile("(?i)" + r.Pcre, r.Id, broken, output))).ToList();
        var requires = rules.ToDictionary(r => r.Spec.Id, r => r.Spec.Requires.Length > 0 ? Compile("(?i)" + r.Spec.Requires, r.Spec.Id, broken, output) : null);
        var originals = lot.CompareOriginal
            ? lot.Rules.Select(r => (r.Id, Rx: Compile("(?i)" + PosixToNet(r.OriginalPcre), r.Id, broken, output))).ToList()
            : [];
        foreach (var obj in lot.Objects)
        {
            try
            {
                var sources = sourcesOf(obj).Where(s => s.Lines.Count > 0).ToList();
                var lines = 0;
                foreach (var src in sources)
                {
                    lines += src.Lines.Count;
                    Scan(obj, src, trigger, rules, requires, output);
                    if (lot.CompareOriginal)
                        Original(obj, src, trigger, originals, output);
                }
                output.Add($"F\t{obj.Type}\t{obj.Name.Trim()}\t{sources.Count}\t{lines}");
            }
            catch (Exception ex) when (ex is RegexMatchTimeoutException or ArgumentException or InvalidOperationException)
            {
                output.Add($"E\t{obj.Type}\t{obj.Name.Trim()}\t{ex.Message}");
            }
        }
        output.Add($"Z\t{lot.Number:D3}\t{lot.Objects.Count}\t{sw.ElapsedMilliseconds}\t0");
        return string.Join('\n', output) + "\n";
    }

    private static Regex? Compile(string pattern, string ruleId, HashSet<string> broken, List<string> output)
    {
        try
        {
            return new Regex(pattern, RegexOptions.CultureInvariant, RegexTimeout);
        }
        catch (ArgumentException ex)
        {
            if (broken.Add(ruleId))
                output.Add($"X\t{ruleId}\t{ex.Message}");
            return null;
        }
    }

    private static void Scan(ScanLotObject obj, SimulatedScanSource src, Regex trigger, List<(ScanRuleSpec Spec, Regex? Rx)> rules, Dictionary<string, Regex?> requires,
        List<string> output)
    {
        if (!src.Lines.Any(trigger.IsMatch))
            return;
        foreach (var s in Sentences(src.Lines))
        {
            if (!trigger.IsMatch(s.WithLiterals))
                continue;
            var hits = new List<(string Rule, string Elem)>();
            foreach (var (spec, rx) in rules)
            {
                if (rx is null)
                    continue;
                var text = spec.Text == "L" ? s.WithLiterals : s.WithoutLiterals;
                foreach (Match m in rx.Matches(text))
                {
                    var elem = spec.FixedElement;
                    if (elem.Length == 0 && m.Groups.Count > 1 && m.Groups[1].Success && m.Groups[1].Length > 0)
                        elem = m.Groups[1].Value;
                    if (!hits.Contains((spec.Id, elem)))
                        hits.Add((spec.Id, elem));
                }
            }
            var trecho = Condense(s.WithLiterals.Replace('\t', ' '));
            if (trecho.Length > 200)
                trecho = trecho[..200];
            foreach (var (rule, elem) in hits)
            {
                var spec = rules.First(r => r.Spec.Id == rule).Spec;
                if (spec.SuppressedBy.Length > 0 && hits.Contains((spec.SuppressedBy, elem)))
                    continue;
                var flag = s.Remediated ? "R" : "P";
                if (flag == "P" && requires[rule] is { } req && !req.IsMatch(s.WithLiterals))
                    flag = "I";
                output.Add($"H\t{obj.Type}\t{obj.Name.Trim()}\t{src.Uri}\t{s.First}\t{s.Last}\t{rule}\t{elem}\t{flag}\t{trecho}");
            }
        }
    }

    private static void Original(ScanLotObject obj, SimulatedScanSource src, Regex trigger, List<(string Id, Regex? Rx)> originals, List<string> output)
    {
        if (!src.Lines.Any(trigger.IsMatch))
            return;
        foreach (var (id, rx) in originals)
        {
            if (rx is null)
                continue;
            var count = 0;
            foreach (var line in src.Lines)
            {
                var m = rx.Match(line);
                if (!m.Success)
                    continue;
                if (line.Length > 0 && (line[0] == '*' || line[0] == '"'))
                    continue;
                var quote = line.IndexOf('"', StringComparison.Ordinal);
                if (quote >= 0 && m.Index > quote)
                    continue;
                count++;
            }
            if (count > 0)
                output.Add($"O\t{obj.Type}\t{obj.Name.Trim()}\t{src.Uri}\t{id}\t{count}");
        }
    }

    internal sealed record Sentence(int First, int Last, string WithLiterals, string WithoutLiterals, bool Remediated);

    /// <summary>
    /// Sentenças: comentários (linha iniciada por <c>*</c> e o que vem depois de <c>"</c>) fora; comandos em várias linhas juntos
    /// (terminam em ponto); cadeias (<c>A: B, C.</c>) separadas em <c>A B.</c> e <c>A C.</c>. Maiúsculas; sem literais = <c>''</c>.
    /// </summary>
    internal static List<Sentence> Sentences(IReadOnlyList<string> lines)
    {
        var ranges = new List<(int Ini, int Fim)>();
        var start = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var up = lines[i].ToUpperInvariant();
            if (!(up.Contains("S/4", StringComparison.Ordinal) && up.Contains("MIGRATION", StringComparison.Ordinal) && up.Contains("OF INSERTION", StringComparison.Ordinal)))
                continue;
            if (up.Contains("START OF INSERTION", StringComparison.Ordinal))
                start = i + 1;
            else if (up.Contains("END OF INSERTION", StringComparison.Ordinal) && start > 0)
            {
                ranges.Add((start, i + 1));
                start = 0;
            }
        }
        var result = new List<Sentence>();
        List<(string Text, int Row)> prefix = [], current = [];
        var chain = false;

        void Emit()
        {
            var tokens = chain ? prefix.Concat(current).ToList() : current;
            if (tokens.Count > 0)
            {
                var com = new StringBuilder();
                var sem = new StringBuilder();
                foreach (var (t, _) in tokens)
                {
                    if (com.Length > 4000 || t.Length == 0)
                        continue;
                    com.Append(' ').Append(t);
                    sem.Append(' ').Append(t[0] is '\'' or '`' ? "''" : t);
                }
                var first = tokens.Min(t => t.Row);
                var last = tokens.Max(t => t.Row);
                result.Add(new Sentence(first, last, com.ToString().ToUpperInvariant(), sem.ToString().ToUpperInvariant(), ranges.Any(r => r.Ini <= first && r.Fim >= first)));
            }
            current = [];
        }

        for (var row = 1; row <= lines.Count; row++)
        {
            var line = lines[row - 1];
            if (line.StartsWith('*'))
                continue;
            foreach (var token in Tokens(line))
            {
                var t = token;
                var literal = t[0] is '\'' or '`' or '|';
                if (!literal && t.EndsWith(':') && !chain)
                {
                    t = t[..^1];
                    if (t.Length > 0)
                        current.Add((t, row));
                    prefix = current;
                    current = [];
                    chain = true;
                    continue;
                }
                if (!literal && t.EndsWith(',') && chain)
                {
                    t = t[..^1];
                    if (t.Length > 0)
                        current.Add((t, row));
                    Emit();
                    continue;
                }
                if (!literal && t.EndsWith('.'))
                {
                    t = t[..^1];
                    if (t.Length > 0)
                        current.Add((t, row));
                    Emit();
                    chain = false;
                    prefix = [];
                    continue;
                }
                current.Add((t, row));
            }
        }
        return result;
    }

    /// <summary>Tokens da linha separados por espaço; literais (<c>'..'</c>, <c>`..`</c>, <c>|..|</c>) inteiros; <c>"</c> fora de literal começa comentário.</summary>
    private static IEnumerable<string> Tokens(string line)
    {
        var i = 0;
        while (i < line.Length)
        {
            if (char.IsWhiteSpace(line[i]))
            {
                i++;
                continue;
            }
            if (line[i] == '"')
                yield break;
            var sb = new StringBuilder();
            while (i < line.Length && !char.IsWhiteSpace(line[i]))
            {
                var c = line[i];
                if (c == '"' && sb.Length == 0)
                    yield break;
                if (c is '\'' or '`' or '|')
                {
                    if (sb.Length > 0)
                        break; // o que veio antes do literal é um token à parte
                    var q = c;
                    sb.Append(c);
                    i++;
                    while (i < line.Length)
                    {
                        sb.Append(line[i]);
                        if (line[i] == q)
                        {
                            if (q != '|' && i + 1 < line.Length && line[i + 1] == q)
                            {
                                sb.Append(line[i + 1]);
                                i += 2;
                                continue;
                            }
                            i++;
                            break;
                        }
                        i++;
                    }
                    break; // o literal é um token inteiro; o que vem depois (ponto, vírgula) começa outro
                }
                sb.Append(c);
                i++;
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }
    }

    internal static string Condense(string s) => SpacesRegex().Replace(s, " ").Trim();

    /// <summary>Classes POSIX do PCRE (<c>[[:space:]]</c>…) para o .NET.</summary>
    internal static string PosixToNet(string pcre) => pcre
        .Replace("[:space:]", @"\s", StringComparison.Ordinal).Replace("[:alnum:]", "a-zA-Z0-9", StringComparison.Ordinal)
        .Replace("[:alpha:]", "a-zA-Z", StringComparison.Ordinal).Replace("[:digit:]", "0-9", StringComparison.Ordinal)
        .Replace("[:upper:]", "A-Z", StringComparison.Ordinal).Replace("[:lower:]", "a-z", StringComparison.Ordinal)
        .Replace("[:punct:]", @"!-/:-@\[-`{-~", StringComparison.Ordinal);

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex SpacesRegex();
}
