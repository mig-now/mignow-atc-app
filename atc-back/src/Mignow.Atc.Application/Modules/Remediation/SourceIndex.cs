using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Os fontes lidos (equivale à pasta <c>fontes/</c> da skill) e as consultas do atc.py sobre eles: <c>locate</c>, <c>uso_em_delecao</c>,
/// <c>forma_chamada</c> e <c>matnr_long_ok</c>. Objeto sem fonte lido = <c>nulo</c> nas consultas (o script também distingue "pasta não
/// existe" de "pasta sem uso"). Os arquivos de um objeto seguem a ordem do <c>glob</c> ordenado (nome + <c>.abap</c>).
/// </summary>
public sealed partial class SourceIndex
{
    private static readonly string[] NotNumberParams = ["material_evg", "material_version", "material_guid"];
    private readonly Dictionary<string, List<(string Name, string[] Lines)>> _byObject = new(StringComparer.OrdinalIgnoreCase);

    public SourceIndex(IEnumerable<SourceFileText> files)
    {
        foreach (var g in files.GroupBy(f => f.ObjectName, StringComparer.OrdinalIgnoreCase))
            _byObject[g.Key] = [.. g.OrderBy(f => f.Name + ".abap", StringComparer.Ordinal).Select(f => (f.Name, SplitLines(f.Text)))];
    }

    public static SourceIndex Empty { get; } = new([]);

    /// <summary>Arquivos do objeto (nome e linhas), na ordem do script.</summary>
    public IReadOnlyList<(string Name, string[] Lines)> Files(string objectName) =>
        _byObject.TryGetValue(objectName, out var files) ? files : [];

    public bool Has(string objectName) => _byObject.ContainsKey(objectName);

    /// <summary>Equivale a <c>str.splitlines()</c> para fim de linha CR, LF e CRLF.</summary>
    public static string[] SplitLines(string text)
    {
        var lines = new List<string>();
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] is not ('\n' or '\r'))
                continue;
            lines.Add(text[start..i]);
            if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                i++;
            start = i + 1;
        }
        if (start < text.Length)
            lines.Add(text[start..]);
        return [.. lines];
    }

    /// <summary>Usos do objeto referenciado fora de comentário (<c>locate</c>); nulo quando nenhum fonte do objeto foi lido.</summary>
    public IReadOnlyList<UseHit>? Locate(string objectName, string reference, int context = 4)
    {
        if (!_byObject.TryGetValue(objectName, out var files))
            return null;
        var pattern = ReferencePattern(reference);
        var hits = new List<UseHit>();
        foreach (var (name, lines) in files)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                var l = lines[i];
                var code = l.StartsWith('*') ? string.Empty : CodePart(l);
                if (code.Length == 0 || !pattern.IsMatch(code.ToUpperInvariant()))
                    continue;
                var a = Math.Max(0, i - context);
                var b = Math.Min(lines.Length, i + context + 1);
                hits.Add(new UseHit(name, i + 1, string.Join('\n', Enumerable.Range(a, b - a).Select(k => $"{k + 1,5}  {lines[k]}"))));
            }
        }
        return hits;
    }

    /// <summary>Linhas ("fonte:linha") em que o objeto só aparece comentado dentro de bloco S/4 MIGRATION DELETION (<c>uso_em_delecao</c>).</summary>
    public IReadOnlyList<string> UsageInDeletion(string objectName, string reference)
    {
        var hits = new List<string>();
        if (!_byObject.TryGetValue(objectName, out var files))
            return hits;
        var pattern = ReferencePattern(reference);
        foreach (var (name, lines) in files)
        {
            var deletion = false;
            for (var i = 0; i < lines.Length; i++)
            {
                var m = MarkerRegex().Match(lines[i]);
                if (m.Success)
                {
                    deletion = m.Groups[3].Value.Equals("DELETION", StringComparison.OrdinalIgnoreCase)
                               && m.Groups[2].Value.Equals("START", StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                if (deletion && lines[i].StartsWith('*') && pattern.IsMatch(lines[i].ToUpperInvariant()))
                    hits.Add($"{name}:{i + 1}");
            }
        }
        return hits;
    }

    /// <summary>Assinatura da forma de uso do objeto referenciado (<c>forma_chamada</c>): agrupa itens iguais.</summary>
    public string CallForm(string objectName, string reference)
    {
        var hits = reference.Length > 0 ? Locate(objectName, reference) : null;
        if (hits is null)
            return "sem_fonte";
        if (hits.Count == 0)
            return "uso_nao_localizado";
        var forms = new SortedSet<string>(StringComparer.Ordinal);
        var re = Regex.Escape(reference.ToUpperInvariant());
        foreach (var h in hits)
        {
            var txt = string.Join(' ', h.Excerpt.Split('\n').Select(l => l.Length > 7 ? l[7..] : string.Empty)).ToUpperInvariant();
            if (Regex.IsMatch(txt, @"CALL\s+FUNCTION\s+'" + re + "'"))
                forms.Add("CALL FUNCTION");
            else if (Regex.IsMatch(txt, @"CALL\s+TRANSACTION\s+'" + re + @"'\s+USING"))
                forms.Add("CALL TRANSACTION USING (BDC)");
            else if (Regex.IsMatch(txt, @"(CALL|LEAVE\s+TO)\s+TRANSACTION\s+'" + re + "'"))
                forms.Add("CALL TRANSACTION");
            else if (Regex.IsMatch(txt, @"SUBMIT\s+" + re))
                forms.Add("SUBMIT");
            else if (Regex.IsMatch(txt, @"(FROM|JOIN|UPDATE|INTO)\s+" + re + @"\b"))
                forms.Add("SQL");
            else if (Regex.IsMatch(txt, @"(TYPE|LIKE)\s+(REF\s+TO\s+|TABLE\s+OF\s+|STANDARD\s+TABLE\s+OF\s+)?" + re + @"\b"))
                forms.Add("TYPE/LIKE");
            else
                forms.Add("outro");
        }
        return string.Join(" + ", forms);
    }

    /// <summary>
    /// R5 (nota 2438131): parâmetros de material de cada CALL FUNCTION ativa do objeto referenciado (<c>matnr_long_ok</c>). Ok quando há
    /// chamada, nenhuma passa parâmetro de material curto e ao menos uma passa <c>*_LONG</c>.
    /// </summary>
    public (bool Ok, IReadOnlyList<string> Calls) MatnrLongOk(string objectName, string reference)
    {
        var calls = new List<string>();
        bool shortParam = false, longParam = false;
        if (!_byObject.TryGetValue(objectName, out var files))
            return (false, calls);
        var call = new Regex(@"CALL\s+FUNCTION\s+'" + Regex.Escape(reference.ToUpperInvariant()) + "'", RegexOptions.CultureInvariant);
        foreach (var (name, lines) in files)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                var l = lines[i];
                if (l.TrimStart().StartsWith('*') || l.TrimStart().StartsWith('"') || !call.IsMatch(l.ToUpperInvariant()))
                    continue;
                var parameters = new List<string>();
                for (var j = i; j < lines.Length; j++)
                {
                    var t = lines[j];
                    if (t.TrimStart().StartsWith('*'))
                        continue;
                    var cod = t.Split('"')[0];
                    var m = ParameterRegex().Match(cod);
                    if (m.Success && MaterialRegex().IsMatch(m.Groups[1].Value) && !NotNumberParams.Contains(m.Groups[1].Value.ToLowerInvariant()))
                        parameters.Add(m.Groups[1].Value.ToLowerInvariant());
                    if (cod.TrimEnd().EndsWith('.'))
                        break;
                }
                shortParam |= parameters.Any(p => !p.Contains("long", StringComparison.Ordinal));
                longParam |= parameters.Any(p => p.Contains("long", StringComparison.Ordinal));
                calls.Add($"{name}:{i + 1} [{(parameters.Count == 0 ? "-" : string.Join(", ", parameters))}]");
            }
        }
        return (calls.Count > 0 && longParam && !shortParam, calls);
    }

    private static string CodePart(string line)
    {
        var q = line.IndexOf('"', StringComparison.Ordinal);
        return q < 0 ? line : line[..q];
    }

    private static Regex ReferencePattern(string reference) =>
        new(@"(?<![A-Z0-9_/])" + Regex.Escape(reference.ToUpperInvariant()) + "(?![A-Z0-9_])", RegexOptions.CultureInvariant);

    [GeneratedRegex(@"^\s*[*""]\s*(>>>|<<<)?[^*]*?S/4\s*MIGRATION.*?-\s*(START|END)\s+OF\s+(DELETION|INSERTION)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MarkerRegex();

    [GeneratedRegex(@"^\s*(\w+)\s*=")]
    private static partial Regex ParameterRegex();

    [GeneratedRegex("material|matnr", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MaterialRegex();
}
