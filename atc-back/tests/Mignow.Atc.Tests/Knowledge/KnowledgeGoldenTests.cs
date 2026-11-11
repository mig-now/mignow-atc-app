using System.Text.Json;
using FluentAssertions;
using Mignow.Atc.Application.Modules.Knowledge;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Infra.External.Knowledge;

namespace Mignow.Atc.Tests.Knowledge;

/// <summary>
/// Paridade com o atc.py original (Golden/knowledge, gerado por <c>gerar.py</c> com <c>python3 -I</c>): kb_lookup, fatos_de,
/// cmd_ddic_lista, alertas/n_comandos/complexidade_card e cmd_registrar/cmd_fato (arquivos produzidos, byte a byte).
/// A KB usada é um subconjunto congelado da real (Golden/knowledge/real_kb), nunca a pasta viva.
/// </summary>
public sealed class KnowledgeGoldenTests
{
    private static readonly string Dir = Path.Combine(AppContext.BaseDirectory, "Golden", "knowledge");

    private static JsonDocument Json(string file) => JsonDocument.Parse(File.ReadAllText(Path.Combine(Dir, file)));

    private static MarkdownKnowledgeBase Kb(string root) => new(new FixedKbPathResolver(root), () => TempKb.FixedDate);

    [Fact]
    public async Task Lookup_matches_python_kb_lookup()
    {
        var kb = Kb(Path.Combine(Dir, "real_kb"));
        using var doc = Json("lookup.json");
        var checkedCards = 0;
        foreach (var q in doc.RootElement.EnumerateArray())
        {
            var robt = q.GetProperty("robt").ValueKind == JsonValueKind.Null ? null : q.GetProperty("robt").GetString();
            var got = await kb.LookupAsync(q.GetProperty("ref").GetString()!, robt, q.GetProperty("top").GetInt32(), default);
            var label = $"{q.GetProperty("ref").GetString()}/{robt}/{q.GetProperty("top").GetInt32()}";
            got.Total.Should().Be(q.GetProperty("total").GetInt32(), label);
            var expected = q.GetProperty("cards").EnumerateArray().ToList();
            got.Cards.Should().HaveCount(expected.Count, label);
            for (var i = 0; i < expected.Count; i++)
            {
                var e = expected[i];
                var c = got.Cards[i];
                c.Id.Should().Be(e.GetProperty("id").GetString(), label);
                c.Occurrences.Should().Be(e.GetProperty("ocorrencias").GetInt32(), label);
                c.Pattern.Should().Be(e.GetProperty("padrao").GetString(), label);
                c.Original.Should().Be(e.GetProperty("original").GetString(), label);
                c.New.Should().Be(e.GetProperty("novo").GetString(), label);
                c.KbSource.Should().Be(e.GetProperty("fonte_kb").GetString(), label);
                c.IsContribution.Should().Be(e.GetProperty("contribuicao").GetBoolean(), label);
                c.Alerts.Should().Equal(e.GetProperty("alertas").EnumerateArray().Select(a => a.GetString()), label);
                c.Complexity.Should().Be(e.GetProperty("complexidade").GetString(), label);
                c.ReplacedBy.Should().Be(e.GetProperty("substituida_por").GetString(), label);
                checkedCards++;
            }
        }
        checkedCards.Should().BeGreaterThan(30);
    }

    [Fact]
    public async Task Facts_match_python_fatos_de()
    {
        var kb = Kb(Path.Combine(Dir, "real_kb"));
        using var doc = Json("facts.json");
        var total = 0;
        foreach (var p in doc.RootElement.EnumerateObject())
        {
            var got = await kb.FactsAsync(p.Name, default);
            got.Select(f => (f.Id, f.Date, f.Text)).Should().Equal(
                p.Value.EnumerateArray().Select(f => (f.GetProperty("id").GetString()!, f.GetProperty("data").GetString()!, f.GetProperty("texto").GetString()!)), p.Name);
            total += got.Count;
        }
        total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DdicCandidates_match_python_cmd_ddic_lista_ignoring_tie_order()
    {
        var kb = Kb(Path.Combine(Dir, "real_kb"));
        using var doc = Json("ddic_lista.json");
        var expected = doc.RootElement.GetProperty("candidatos").EnumerateArray()
            .Select(c => (c.GetProperty("tipo").GetString()!, c.GetProperty("nome").GetString()!, c.GetProperty("ocorrencias").GetInt32(), c.GetProperty("em_cache").GetBoolean())).ToList();

        var got = await kb.DdicCandidatesAsync(1000, default);

        got.Candidates.Select(c => (c.Type, c.Name, c.Occurrences, c.Cached)).Should().BeEquivalentTo(expected);
        got.Candidates.Select(c => c.Occurrences).Should().BeInDescendingOrder();
        got.MissingFromCache.Should().BeEquivalentTo(doc.RootElement.GetProperty("faltando_no_cache").EnumerateArray().Select(x => x.GetString()));
        expected.Should().NotBeEmpty();
    }

    [Fact]
    public void Code_rules_match_python_alertas_n_comandos_complexidade()
    {
        using var doc = Json("code_rules.json");
        foreach (var e in doc.RootElement.EnumerateArray())
        {
            var code = e.GetProperty("new").GetString()!;
            var original = e.GetProperty("original").GetString()!;
            var alerts = KbCodeRules.Alerts(code, original);
            alerts.Should().Equal(e.GetProperty("alerts").EnumerateArray().Select(a => a.GetString()), code);
            KbCodeRules.CommandCount(code).Should().Be(e.GetProperty("commands").GetInt32(), code);
            KbCodeRules.Complexity(alerts, original, code).Should().Be(e.GetProperty("complexity").GetString(), code);
        }
    }

    [Fact]
    public async Task Register_and_AddFact_produce_the_same_files_as_python()
    {
        using var kbDir = new TempKb();
        Directory.CreateDirectory(Path.Combine(kbDir.Root, "contribuicoes", "funcoes"));
        Directory.CreateDirectory(Path.Combine(kbDir.Root, "contribuicoes", "fatos"));
        Directory.CreateDirectory(Path.Combine(kbDir.Root, "contribuicoes", "tabelas"));
        // mesma semente do gerar.py: json.dumps(indent=1)
        File.WriteAllText(Path.Combine(kbDir.Root, "contribuicoes", "substituicoes.json"),
            "{\n \"S1\": {\n  \"por\": \"C-X-001\",\n  \"elemento\": \"X\",\n  \"data\": \"2026-01-01\",\n  \"motivo\": \"m\"\n }\n}");
        var kb = kbDir.Create();

        using (var steps = Json("write_steps.json"))
        {
            foreach (var s in steps.RootElement.EnumerateArray())
            {
                string? Str(string name) => s.GetProperty(name).ValueKind == JsonValueKind.Null ? null : s.GetProperty(name).GetString();
                var replaces = (Str("substitui") ?? "").Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                await kb.RegisterAsync(new KbContribution(Str("categoria")!, Str("elemento")!, Str("problema"), Str("before")!, Str("after")!, replaces, Str("motivo")), default);
            }
        }
        await kb.AddFactAsync(new KbFactInput(["BAPI_TESTE", " /X/Y ", ""], "Comportamento Z", "prova P", "impacto I"), default);
        await kb.AddFactAsync(new KbFactInput(["bapi_teste"], "Outro fato", null, null), default);

        var expectedRoot = Path.Combine(Dir, "write_expected");
        var expectedFiles = Directory.EnumerateFiles(expectedRoot, "*", SearchOption.AllDirectories).Select(f => Path.GetRelativePath(expectedRoot, f)).Order().ToList();
        var actualFiles = Directory.EnumerateFiles(kbDir.Root, "*", SearchOption.AllDirectories).Select(f => Path.GetRelativePath(kbDir.Root, f)).Order().ToList();
        actualFiles.Should().Equal(expectedFiles);
        foreach (var rel in expectedFiles)
        {
            var expected = File.ReadAllText(Path.Combine(expectedRoot, rel)).Replace("{{DATE}}", TempKb.FixedDate.ToString("yyyy-MM-dd"));
            File.ReadAllText(Path.Combine(kbDir.Root, rel)).Should().Be(expected, rel);
        }
    }
}
