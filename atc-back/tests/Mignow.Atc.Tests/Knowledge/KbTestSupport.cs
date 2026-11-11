using Mignow.Atc.Infra.External.Knowledge;

namespace Mignow.Atc.Tests.Knowledge;

internal sealed class FixedKbPathResolver(string root) : IKbPathResolver
{
    public string Root { get; set; } = root;

    public Task<string> ResolveRootAsync(CancellationToken ct) => Task.FromResult(Root);
}

/// <summary>Pasta temporária com uma mini-KB sintética (nunca toca a KB real).</summary>
internal sealed class TempKb : IDisposable
{
    public static readonly DateOnly FixedDate = new(2030, 1, 2);

    public TempKb()
    {
        Root = Path.Combine(Path.GetTempPath(), "kbtest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    public MarkdownKnowledgeBase Create() => new(new FixedKbPathResolver(Root), () => FixedDate);

    public string Write(string relative, string content)
    {
        var path = Path.Combine(Root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    public static string Card(string id, int occ, string original, string @new, string pattern = "[Tabela substituída](../../padroes/P02.md)") =>
        $"<a id=\"{id.ToLowerInvariant()}\"></a>\n### {id}\n\n**Padrão:** {pattern}  \n**Ocorrências:** {occ}  \n\n"
        + $"**Problema (código original)**\n\n```abap\n{original}\n```\n\n**Solução (código novo)**\n\n```abap\n{@new}\n```\n\n---\n";

    public static string File_(string title, params string[] cards) => $"# {title}\n\n**Categoria:** x  \n\n---\n" + string.Concat(cards);

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, true);
        }
        catch (IOException)
        {
        }
    }
}
