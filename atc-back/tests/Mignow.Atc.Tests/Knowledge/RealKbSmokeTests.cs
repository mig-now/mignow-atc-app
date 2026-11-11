using FluentAssertions;
using Mignow.Atc.Infra.External.Knowledge;

namespace Mignow.Atc.Tests.Knowledge;

/// <summary>Leitura SOMENTE LEITURA da KB real do repositório (nunca escreve); ignorado se a pasta não existir.</summary>
public sealed class RealKbSmokeTests
{
    [Fact]
    public async Task Real_kb_loads_browses_and_looks_up()
    {
        var repo = KbPaths.FindUp(Path.Combine(".claude", "skills", "atc-remediacao", "KB_ATC"), AppContext.BaseDirectory);
        if (repo is null)
            return;
        var root = KbPaths.Resolve("../.claude/skills/atc-remediacao/KB_ATC", repo);
        var kb = new MarkdownKnowledgeBase(new FixedKbPathResolver(root), () => TempKb.FixedDate);

        var index = await kb.BrowseAsync(default);
        var patterns = await kb.BrowsePatternsAsync(default);
        var lookup = await kb.LookupAsync("BAPI_PO_CREATE", "FUNC", 3, default);

        index.Count.Should().BeGreaterThan(100);
        index.Sum(e => e.Cards).Should().BeGreaterThan(1000);
        patterns.Should().NotBeEmpty();
        lookup.Total.Should().BeGreaterThan(0);
        (await kb.GetElementAsync(index.First(e => e.Category == "funcoes").Category, index.First(e => e.Category == "funcoes").Slug, default)).Should().NotBeNull();
    }
}
