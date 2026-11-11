namespace Mignow.Atc.Infra.External.Knowledge;

/// <summary>Resolução robusta da pasta da KB: o caminho configurado é absoluto ou relativo à raiz do repositório (ou do atc-back).</summary>
public static class KbPaths
{
    /// <summary>Sobe a partir de cada ponto de partida até achar a pasta que contém <paramref name="marker"/> (arquivo ou pasta).</summary>
    public static string? FindUp(string marker, params string?[] starts)
    {
        foreach (var start in starts.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            var dir = new DirectoryInfo(start!);
            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, marker);
                if (Directory.Exists(candidate) || File.Exists(candidate))
                    return dir.FullName;
                dir = dir.Parent;
            }
        }
        return null;
    }

    /// <summary>
    /// Pasta da KB: se o caminho é absoluto, ele; senão o primeiro candidato existente entre raiz do repositório
    /// (override de configuração ou detectada), raiz do atc-back (padrão <c>../.claude/…</c>), diretório atual e pasta do binário.
    /// Nenhum existindo, devolve o primeiro candidato (o erro sai na leitura, com o caminho tentado).
    /// </summary>
    public static string Resolve(string configured, string? repoRootOverride = null)
    {
        if (Path.IsPathRooted(configured))
            return Path.GetFullPath(configured);
        var baseDir = AppContext.BaseDirectory;
        var cwd = Directory.GetCurrentDirectory();
        var repoRoot = !string.IsNullOrWhiteSpace(repoRootOverride)
            ? repoRootOverride
            : FindUp(Path.Combine(".claude", "skills", "atc-remediacao"), cwd, baseDir);
        var backRoot = FindUp("Mignow.Atc.slnx", cwd, baseDir);
        var candidates = new[] { repoRoot, backRoot, cwd, baseDir }
            .Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => Path.GetFullPath(Path.Combine(c!, configured))).ToList();
        return candidates.FirstOrDefault(Directory.Exists) ?? candidates.FirstOrDefault() ?? Path.GetFullPath(configured);
    }
}
