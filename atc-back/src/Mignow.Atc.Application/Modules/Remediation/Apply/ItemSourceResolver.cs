using System.Text.Json;
using System.Text.RegularExpressions;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Fonte físico do item (porte da <c>unidade</c> do <c>plano-paralelo</c>): pela <c>location</c> do finding (coluna do CSV da sessão:
/// <c>location</c>/<c>Fonte ADT</c>, com <c>#start=linha</c>; sem a coluna, a da extração pelo Finding ID); sem ela, o fonte do objeto onde o objeto referenciado é usado (<c>locate</c>);
/// sem uso, o único fonte do objeto ou o principal. A linha vem da <c>location</c> ou do primeiro uso.
/// </summary>
public sealed partial class ItemSourceResolver
{
    private readonly Dictionary<string, List<SourceCache>> _byObject;
    private readonly SourceIndex _index;
    private readonly int _locationColumn;
    private readonly int _lineColumn;
    private readonly IReadOnlyDictionary<string, string> _locations;

    /// <param name="locations">Finding ID → <c>location</c> da extração (<c>extracao_por_finding</c>), usada quando a linha do CSV não tem a coluna.</param>
    public ItemSourceResolver(IReadOnlyList<SourceCache> caches, IReadOnlyList<string> columns, IReadOnlyDictionary<string, string>? locations = null)
    {
        _locations = locations ?? new Dictionary<string, string>();
        _byObject = caches.GroupBy(c => c.ObjectName, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        _index = new SourceIndex(caches.Select(c => new SourceFileText(c.ObjectName, c.IncludeName, c.Source)));
        _locationColumn = IndexOf(columns, "location", "Fonte ADT");
        _lineColumn = IndexOf(columns, "Linha", "Line");
    }

    public SourceIndex Index => _index;

    public (SourceCache? Cache, int? Line) Resolve(RemediationItem item)
    {
        if (!_byObject.TryGetValue(item.ObjectName, out var caches) || caches.Count == 0)
            return (null, null);
        string[] row = Row(item);
        int? rowLine = null;
        if (_lineColumn >= 0 && _lineColumn < row.Length && int.TryParse(row[_lineColumn], out var l))
            rowLine = l;
        var location = _locationColumn >= 0 && _locationColumn < row.Length && row[_locationColumn].Length > 0
            ? row[_locationColumn]
            : item.FindingId is { Length: > 0 } fid && _locations.TryGetValue(fid, out var fromExtraction) ? fromExtraction : null;
        if (location is not null && LocationRegex().Match(location) is { Success: true } m)
        {
            var key = Uri.UnescapeDataString(m.Groups[1].Value).ToLowerInvariant();
            var found = caches.FirstOrDefault(c => Uri.UnescapeDataString(c.AdtUri.Split('?', '#')[0]).ToLowerInvariant() == key);
            if (found is not null)
                return (found, m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : rowLine);
        }
        if (!string.IsNullOrEmpty(item.RefObject) && _index.Locate(item.ObjectName, item.RefObject) is { Count: > 0 } hits)
        {
            var cache = caches.FirstOrDefault(c => c.IncludeName == hits[0].Source);
            if (cache is not null)
                return (cache, rowLine ?? hits[0].Line);
        }
        var main = caches.FirstOrDefault(c => string.Equals(c.IncludeName, item.ObjectName, StringComparison.OrdinalIgnoreCase)) ?? caches[0];
        return (main, rowLine);
    }

    private static string[] Row(RemediationItem item)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(item.RawRowJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static int IndexOf(IReadOnlyList<string> columns, params string[] names)
    {
        for (var i = 0; i < columns.Count; i++)
            if (names.Any(n => string.Equals(columns[i].Trim(), n, StringComparison.OrdinalIgnoreCase)))
                return i;
        return -1;
    }

    [GeneratedRegex(@"(/sap/bc/adt/\S+?/source/main)(?:\?[^#\s]*)?(?:#start=(\d+))?", RegexOptions.IgnoreCase)]
    private static partial Regex LocationRegex();
}
