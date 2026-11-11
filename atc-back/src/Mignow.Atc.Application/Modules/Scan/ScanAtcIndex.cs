using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// Índice dos findings do ATC por fonte (porte de <c>chave_fonte</c> e da montagem de <c>idx</c> do <c>cruzar.py</c>): chave =
/// caminho do fonte em minúsculas, sem <c>/sap/bc/adt/</c>, <c>/source/main</c>, query e fragmento; valor = linha inicial
/// (<c>#start=</c>, nula se não houver) e elemento (<c>tag_REF_OBJ_NAME</c> em maiúsculas).
/// </summary>
public sealed partial class ScanAtcIndex
{
    private readonly Dictionary<string, List<(int? Line, string Ref)>> _index = new(StringComparer.Ordinal);

    public ScanAtcIndex(IEnumerable<ScanAtcFinding> findings)
    {
        foreach (var f in findings)
        {
            if (string.IsNullOrEmpty(f.Location))
                continue;
            var m = StartRegex().Match(f.Location);
            var key = SourceKey(f.Location);
            if (!_index.TryGetValue(key, out var list))
                _index[key] = list = [];
            list.Add((m.Success ? int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : null, f.RefObjName.ToUpperInvariant()));
        }
    }

    /// <summary>Findings (linha, elemento) do fonte; vazio se o ATC não tem finding nele.</summary>
    public IReadOnlyList<(int? Line, string Ref)> For(string source) => _index.TryGetValue(SourceKey(source), out var l) ? l : [];

    /// <summary>location do ADT → caminho do fonte comparável com o da varredura.</summary>
    public static string SourceKey(string loc)
    {
        var p = loc.Split('#')[0].Split('?')[0];
        p = Uri.UnescapeDataString(p).ToLowerInvariant();
        const string adt = "/sap/bc/adt/";
        var i = p.IndexOf(adt, StringComparison.Ordinal);
        if (i >= 0)
            p = p.Remove(i, adt.Length);
        p = p.TrimStart('/');
        return p.EndsWith("/source/main", StringComparison.Ordinal) ? p[..^"/source/main".Length] : p;
    }

    [GeneratedRegex(@"#start=(\d+)")]
    private static partial Regex StartRegex();
}
