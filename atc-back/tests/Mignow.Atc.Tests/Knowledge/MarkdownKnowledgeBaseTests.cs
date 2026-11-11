using FluentAssertions;
using Mignow.Atc.Application.Ports.Knowledge;

namespace Mignow.Atc.Tests.Knowledge;

public sealed class MarkdownKnowledgeBaseTests : IDisposable
{
    private readonly TempKb _kb = new();

    public void Dispose() => _kb.Dispose();

    [Fact]
    public async Task Lookup_orders_contributions_first_then_unreplaced_then_no_alert_then_occurrences()
    {
        _kb.Write("elementos/tabelas/VBUK.md", TempKb.File_("VBUK",
            TempKb.Card("S1", 5, "SELECT * FROM vbuk.", "SELECT * FROM vbak."),
            TempKb.Card("S2", 9, "SELECT * FROM vbuk.", "CALL FUNCTION 'CONVERSION_EXIT_ALPHA_INPUT'\n EXPORTING input = matnr.\n"),
            TempKb.Card("S3", 2, "SELECT * FROM vbuk.", "SELECT * FROM vbap."),
            TempKb.Card("S4", 7, "SELECT * FROM vbuk.", "SELECT * FROM vbep."),
            TempKb.Card("S5", 1, "SELECT * FROM vbuk.", "SELECT * FROM vbfa.")));
        _kb.Write("contribuicoes/tabelas/VBUK.md", TempKb.File_("VBUK", TempKb.Card("C-VBUK-001", 1, "a.", "b.", "Contribuição (IA confirmada)")));
        _kb.Write("contribuicoes/substituicoes.json", "{\n \"S4\": {\n  \"por\": \"C-VBUK-001\",\n  \"elemento\": \"VBUK\",\n  \"data\": \"2026-10-22\",\n  \"motivo\": \"porque\"\n }\n}");
        var kb = _kb.Create();

        var r = await kb.LookupAsync("vbuk", "TABL", 10, default);

        r.Total.Should().Be(6);
        r.Cards.Select(c => c.Id).Should().Equal("C-VBUK-001", "S1", "S3", "S5", "S2", "S4");
        r.Cards[0].IsContribution.Should().BeTrue();
        r.Cards[0].KbSource.Should().Be("contribuicoes/tabelas/VBUK.md");
        var replaced = r.Cards.Single(c => c.Id == "S4");
        replaced.ReplacedBy.Should().Be("C-VBUK-001");
        replaced.Alerts[0].Should().Be("Solução substituída por C-VBUK-001 em 2026-10-22: porque");
        replaced.Complexity.Should().Be("simples", "a marca de substituída entra depois do cálculo, como no kb_lookup");
        r.Cards.Single(c => c.Id == "S2").Alerts.Should().ContainSingle().Which.Should().StartWith("Conversão ALPHA aplicada a material");
        (await kb.LookupAsync("VBUK", "TABL", 2, default)).Cards.Should().HaveCount(2);
    }

    [Fact]
    public async Task Lookup_uses_robt_category_first_and_stops_at_first_category_with_cards()
    {
        _kb.Write("elementos/funcoes/FOO.md", TempKb.File_("FOO", TempKb.Card("S1", 1, "a.", "b.")));
        _kb.Write("elementos/tabelas/FOO.md", TempKb.File_("FOO", TempKb.Card("S2", 1, "a.", "b.")));
        var kb = _kb.Create();

        (await kb.LookupAsync("FOO", "TABL", 3, default)).Cards.Single().Id.Should().Be("S2");
        (await kb.LookupAsync("FOO", "FUNC", 3, default)).Cards.Single().Id.Should().Be("S1");
        (await kb.LookupAsync("FOO", null, 3, default)).Cards.Single().Id.Should().Be("S2", "sem RObT vale a ordem CATS: tabelas primeiro");
    }

    [Fact]
    public async Task Lookup_slug_replaces_non_alphanumerics_and_missing_returns_empty()
    {
        _kb.Write("elementos/classes/_SCWM_CL_TM.md", TempKb.File_("/SCWM/CL_TM", TempKb.Card("S1", 3, "a.", "b.")));
        var kb = _kb.Create();

        (await kb.LookupAsync("/scwm/cl_tm", "CLAS", 3, default)).Cards.Should().ContainSingle();
        (await kb.LookupAsync("NAO_EXISTE", "CLAS", 3, default)).Should().BeEquivalentTo(new KbLookupResult([], 0));
    }

    [Fact]
    public async Task Lookup_skips_blocks_without_two_code_samples()
    {
        _kb.Write("elementos/tabelas/X.md", "# X\n---\n<a id=\"s1\"></a>\n### S1\n\n**Ocorrências:** 2  \n\n```abap\nsó um\n```\n");
        (await _kb.Create().LookupAsync("X", "TABL", 3, default)).Total.Should().Be(0);
    }

    [Fact]
    public async Task Cache_reloads_when_file_changes()
    {
        var path = _kb.Write("elementos/tabelas/X.md", TempKb.File_("X", TempKb.Card("S1", 1, "a.", "b.")));
        var kb = _kb.Create();
        (await kb.LookupAsync("X", "TABL", 5, default)).Total.Should().Be(1);

        File.WriteAllText(path, TempKb.File_("X", TempKb.Card("S1", 1, "a.", "b."), TempKb.Card("S2", 1, "a.", "b.")));
        (await kb.LookupAsync("X", "TABL", 5, default)).Total.Should().Be(2);
    }

    [Fact]
    public async Task Register_creates_file_numbers_cards_and_records_substitutions()
    {
        var kb = _kb.Create();

        var id1 = await kb.RegisterAsync(new KbContribution("funcoes", "bapi_x", "Falha", "antes.\n\n", "depois.  ", ["S00001", " S00002 "], "motivo"), default);
        var id2 = await kb.RegisterAsync(new KbContribution("funcoes", "BAPI_X", null, "a.", "b.", [], null), default);

        id1.Should().Be("C-BAPI_X-001");
        id2.Should().Be("C-BAPI_X-002");
        var md = File.ReadAllText(Path.Combine(_kb.Root, "contribuicoes/funcoes/BAPI_X.md"));
        md.Should().StartWith("# BAPI_X\n\n**Categoria:** funcoes  \n");
        md.Should().Contain("**Registrado em:** 2030-01-02  ").And.Contain("**Substitui:** S00001, S00002  ").And.Contain("```abap\ndepois.\n```");
        File.ReadAllText(Path.Combine(_kb.Root, "contribuicoes/substituicoes.json")).Should().Contain("\"S00002\": {\n  \"por\": \"C-BAPI_X-001\"");
        File.ReadAllText(Path.Combine(_kb.Root, "contribuicoes/README.md")).Should().Contain("| [BAPI_X](funcoes/BAPI_X.md) | funcoes | 2 |");

        var lookup = await kb.LookupAsync("BAPI_X", "FUNC", 5, default);
        lookup.Cards.Select(c => c.Id).Should().Equal("C-BAPI_X-001", "C-BAPI_X-002");
    }

    [Fact]
    public async Task Register_never_rewrites_existing_cards()
    {
        var original = TempKb.File_("BAPI_Y", TempKb.Card("C-BAPI_Y-001", 1, "a.", "b."));
        var path = _kb.Write("contribuicoes/funcoes/BAPI_Y.md", original);

        var id = await _kb.Create().RegisterAsync(new KbContribution("funcoes", "BAPI_Y", null, "x.", "y.", [], null), default);

        id.Should().Be("C-BAPI_Y-002");
        File.ReadAllText(path).Should().StartWith(original);
    }

    [Fact]
    public async Task Register_rejects_invalid_category()
    {
        var act = () => _kb.Create().RegisterAsync(new KbContribution("fatos", "X", null, "a", "b", [], null), default);
        (await act.Should().ThrowAsync<KbException>()).Which.Code.Should().Be("Knowledge.InvalidCategory");
    }

    [Fact]
    public async Task Register_is_safe_under_concurrency_and_numbers_are_unique()
    {
        var kb1 = _kb.Create();
        var kb2 = _kb.Create();
        var ids = await Task.WhenAll(Enumerable.Range(0, 24).Select(i =>
            (i % 2 == 0 ? kb1 : kb2).RegisterAsync(new KbContribution("tabelas", "CONC", null, $"a{i}.", $"b{i}.", [], null), default)));

        ids.Should().OnlyHaveUniqueItems();
        ids.Order().Should().Equal(Enumerable.Range(1, 24).Select(n => $"C-CONC-{n:000}"));
        (await kb1.LookupAsync("CONC", "TABL", 100, default)).Total.Should().Be(24);
    }

    [Fact]
    public async Task Register_rereads_file_changed_by_another_writer_before_numbering()
    {
        var kb = _kb.Create();
        await kb.RegisterAsync(new KbContribution("tabelas", "REREAD", null, "a.", "b.", [], null), default);
        var path = Path.Combine(_kb.Root, "contribuicoes/tabelas/REREAD.md");
        File.AppendAllText(path, TempKb.Card("C-REREAD-002", 1, "x.", "y.", "Contribuição (IA confirmada)")); // skill Python escreveu por fora

        (await kb.RegisterAsync(new KbContribution("tabelas", "REREAD", null, "a.", "b.", [], null), default)).Should().Be("C-REREAD-003");
    }

    [Fact]
    public async Task AddFact_numbers_per_element_and_skips_blank_elements()
    {
        var kb = _kb.Create();

        var ids = await kb.AddFactAsync(new KbFactInput(["bapi_a", " ", "/X/Y"], "Comportamento", "prova", "impacto"), default);
        var ids2 = await kb.AddFactAsync(new KbFactInput(["BAPI_A"], "Outro", null, null), default);

        ids.Should().Equal("F-BAPI_A-001", "F-_X_Y-001");
        ids2.Should().Equal("F-BAPI_A-002");
        var facts = await kb.FactsAsync("bapi_a", default);
        facts.Select(f => f.Id).Should().Equal("F-BAPI_A-001", "F-BAPI_A-002");
        facts[0].Date.Should().Be("2030-01-02");
        facts[0].Text.Should().Contain("**Fato:** Comportamento").And.Contain("**Prova:** prova").And.Contain("**Impacto/como usar:** impacto");
        File.ReadAllText(Path.Combine(_kb.Root, "contribuicoes/fatos/README.md")).Should().Contain("| [BAPI_A](BAPI_A.md) | 2 |");
    }

    [Fact]
    public async Task Browse_and_GetElement_join_cards_contributions_facts_and_replacements()
    {
        _kb.Write("elementos/tabelas/_ARMP_X.md", TempKb.File_("/ARMP/X", TempKb.Card("S1", 1, "a.", "b."), TempKb.Card("S2", 1, "a.", "b.")));
        _kb.Write("elementos/funcoes/ONLYFACT.md", TempKb.File_("ONLYFACT", TempKb.Card("S3", 1, "a.", "b.")));
        var kb = _kb.Create();
        await kb.RegisterAsync(new KbContribution("tabelas", "/ARMP/X", null, "a.", "b.", ["S1"], "m"), default);
        await kb.AddFactAsync(new KbFactInput(["_ARMP_X", "SOFATO"], "f", null, null), default);

        var index = await kb.BrowseAsync(default);

        index.Should().ContainEquivalentOf(new KbIndexEntry("tabelas", "/ARMP/X", "_ARMP_X", 2, 1, 1));
        index.Should().ContainEquivalentOf(new KbIndexEntry("funcoes", "ONLYFACT", "ONLYFACT", 1, 0, 0));
        index.Should().ContainEquivalentOf(new KbIndexEntry("fatos", "SOFATO", "SOFATO", 0, 0, 1));

        var detail = await kb.GetElementAsync("tabelas", "/ARMP/X", default);
        detail!.Element.Should().Be("/ARMP/X");
        detail.Cards.Select(c => c.Id).Should().Equal("S1", "S2");
        detail.Contributions.Select(c => c.Id).Should().Equal("C-_ARMP_X-001");
        detail.Facts.Should().ContainSingle();
        detail.Replacements.Should().ContainSingle().Which.Should().Be(new KbReplacement("S1", "C-_ARMP_X-001", "/ARMP/X", "2030-01-02", "m"));
        (await kb.GetElementAsync("tabelas", "_ARMP_X", default))!.Cards.Should().HaveCount(2);
        (await kb.GetElementAsync("tabelas", "NAO", default)).Should().BeNull();
        (await kb.GetElementAsync("../etc", "passwd", default)).Should().BeNull();
        (await kb.GetElementAsync("fatos", "SOFATO", default))!.Facts.Should().ContainSingle();
    }

    [Fact]
    public async Task BrowsePatterns_reads_the_index_table()
    {
        _kb.Write("indices/por_padrao.md", "# Índice por padrão\n\n| Padrão | Descrição | Soluções | Ocorrências |\n|---|---|---|---|\n| [P01](../padroes/P01.md) | Função substituída | 206 | 683 |\n| [P99](../padroes/P99.md) | Não classificado | 4007 | 5916 |\n");
        (await _kb.Create().BrowsePatternsAsync(default)).Should().Equal(new KbPatternEntry("P01", "Função substituída", 206, 683), new KbPatternEntry("P99", "Não classificado", 4007, 5916));
    }

    [Fact]
    public async Task Ddic_lists_and_reads_cache_read_only()
    {
        _kb.Write("ddic/funcoes/BAPI_A.abap", "FUNCTION bapi_a.\r\nENDFUNCTION.");
        _kb.Write("ddic/estruturas/BAPIX.abap", "x");
        var kb = _kb.Create();

        (await kb.ListDdicAsync(default)).Should().Equal(new KbDdicEntry("estruturas", "BAPIX"), new KbDdicEntry("funcoes", "BAPI_A"));
        (await kb.GetDdicAsync("funcoes", "BAPI_A", default)).Should().Be("FUNCTION bapi_a.\nENDFUNCTION.");
        (await kb.GetDdicAsync("funcoes", "../x", default)).Should().BeNull();
        (await kb.GetDdicAsync("outro", "BAPI_A", default)).Should().BeNull();
    }

    [Fact]
    public async Task Missing_kb_folder_throws_KbException()
    {
        var resolver = new FixedKbPathResolver(Path.Combine(_kb.Root, "nao-existe"));
        var kb = new Mignow.Atc.Infra.External.Knowledge.MarkdownKnowledgeBase(resolver, () => TempKb.FixedDate);
        (await ((Func<Task>)(() => kb.LookupAsync("X", null, 3, default))).Should().ThrowAsync<KbException>()).Which.Code.Should().Be("Knowledge.KbNotFound");
    }
}
