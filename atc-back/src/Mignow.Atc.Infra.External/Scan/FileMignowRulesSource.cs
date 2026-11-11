using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Scan;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Infra.External.Scan;

/// <summary>
/// Lê o <c>regras.json</c> da skill (somente leitura). Caminho absoluto, ou relativo: tenta a partir da pasta atual e de cada pasta
/// acima dela e do executável (a raiz do repositório e a pasta <c>atc-back/</c> são ambas aceitas, pois o padrão
/// <c>../.claude/skills/…</c> é relativo a <c>atc-back/</c>).
/// </summary>
public sealed class FileMignowRulesSource(IStringLocalizer<Errors> l) : IMignowRulesSource
{
    public async Task<Result<string>> ReadAsync(string path, CancellationToken ct)
    {
        foreach (var candidate in Candidates(path))
            if (File.Exists(candidate))
                return await File.ReadAllTextAsync(candidate, ct);
        return l.NotFound("Scan.RulesNotFound", path);
    }

    internal static IEnumerable<string> Candidates(string path)
    {
        if (Path.IsPathRooted(path))
        {
            yield return path;
            yield break;
        }
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
                yield return Path.GetFullPath(Path.Combine(dir.FullName, path));
    }
}
