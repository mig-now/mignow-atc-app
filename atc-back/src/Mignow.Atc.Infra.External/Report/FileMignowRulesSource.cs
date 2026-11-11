using System.Text.Json;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Infra.External.Report;

/// <summary>
/// Lê o <c>regras.json</c> da atc-extracao-mignow. O caminho (<c>GeneralSettings.MignowRulesPath</c>) é relativo à raiz do
/// repositório: procura subindo a partir do diretório do aplicativo e do diretório atual. Arquivo ausente = sem regras.
/// </summary>
public sealed class FileMignowRulesSource : IMignowRulesSource
{
    public async Task<IReadOnlyDictionary<string, MignowRuleInfo>> LoadAsync(string path, CancellationToken ct)
    {
        var file = Resolve(path);
        if (file is null)
            return new Dictionary<string, MignowRuleInfo>();
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(file, ct));
        var rules = new Dictionary<string, MignowRuleInfo>(StringComparer.Ordinal);
        if (!doc.RootElement.TryGetProperty("regras", out var list) || list.ValueKind != JsonValueKind.Array)
            return rules;
        foreach (var r in list.EnumerateArray())
        {
            var id = Str(r, "id");
            if (id is null)
                continue;
            string? pcre = null, fixedElement = null;
            if (r.TryGetProperty("refinada", out var rf) && rf.ValueKind == JsonValueKind.Object)
            {
                pcre = Str(rf, "pcre");
                fixedElement = Str(rf, "elemento_fixo");
            }
            rules[id] = new MignowRuleInfo(id, Str(r, "categoria"), Str(r, "severidade"), Str(r, "descricao"), pcre, fixedElement);
        }
        return rules;
    }

    private static string? Str(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static string? Resolve(string path)
    {
        if (Path.IsPathRooted(path))
            return File.Exists(path) ? path : null;
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
            {
                var candidate = Path.GetFullPath(Path.Combine(dir.FullName, path));
                if (File.Exists(candidate))
                    return candidate;
                // o padrão é "../.claude/...": relativo a uma pasta filha da raiz (atc-back)
                var trimmed = path.StartsWith("../", StringComparison.Ordinal) ? path[3..] : path;
                candidate = Path.Combine(dir.FullName, trimmed);
                if (File.Exists(candidate))
                    return candidate;
            }
        }
        return null;
    }
}
