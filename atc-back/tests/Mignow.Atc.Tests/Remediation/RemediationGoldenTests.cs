using System.Text.Json;
using FluentAssertions;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using NSubstitute;

namespace Mignow.Atc.Tests.Remediation;

/// <summary>Paridade com o atc.py original (importar, triagem, grupos, exportar) sobre entradas sintéticas; regerar com <c>Golden/remediation/gerar.sh</c>.</summary>
public sealed class RemediationGoldenTests
{
    [Fact]
    public void Importar_bate_com_o_script()
    {
        var (_, outcome, _) = GoldenFixture.Import();
        var e = GoldenFixture.Expected("importar.json");
        var s = outcome.Summary!;
        s.Items.Should().Be(e.GetProperty("itens").GetInt32());
        s.ByType.Should().BeEquivalentTo(e.GetProperty("por_tipo").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32()));
        s.DistinctObjects.Should().Be(e.GetProperty("objetos_distintos").GetInt32());
        s.ByPriorityInCsv.Should().BeEquivalentTo(e.GetProperty("por_prioridade_csv").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32()));
        s.OutOfScope.Should().Be(e.GetProperty("fora_do_escopo").GetInt32());
        s.ShiftedColumns.Should().Be(e.GetProperty("linhas_com_colunas_deslocadas").GetInt32());
        foreach (var a in e.GetProperty("amostra").EnumerateArray())
        {
            var r = outcome.Rows.Single(x => x.N == a.GetProperty("n").GetInt32());
            new[] { r.ObjectName, r.ObjectType, r.RefType, r.Ref }.Should().Equal(a.GetProperty("objeto").GetString(), a.GetProperty("tipo").GetString(),
                a.GetProperty("robt").GetString(), a.GetProperty("ref").GetString());
        }
    }

    [Theory]
    [InlineData("triagem_conversao.json", ProjectType.Conversion)]
    [InlineData("triagem_upgrade.json", ProjectType.Upgrade)]
    public void Triagem_bate_com_o_script_em_todas_as_regras(string file, ProjectType type)
    {
        var (_, _, items) = GoldenFixture.Import();
        var result = GoldenFixture.Triage(items, type);
        var e = GoldenFixture.Expected(file);
        var codes = e.EnumerateObject().Where(p => p.Value.ValueKind == JsonValueKind.Array && p.Name != "aplicados").Select(p => p.Name).ToList();
        // o script lista todas as regras; o app só as do tipo de projeto (R9 é de upgrade: em conversão ela não entra, e o script a deixa vazia)
        var applicable = TriageEngine.RulesFor(type, new DefaultProductRules().Get()).Select(r => r.Code).ToHashSet();
        foreach (var outside in codes.Where(c => !applicable.Contains(c)))
            e.GetProperty(outside).GetArrayLength().Should().Be(0, outside);
        result.Rules.Select(r => r.Code).Should().Equal(codes.Where(applicable.Contains));
        foreach (var rule in result.Rules)
        {
            var expected = e.GetProperty(rule.Code).EnumerateArray().ToList();
            rule.Items.Should().HaveCount(expected.Count, rule.Code);
            for (var i = 0; i < expected.Count; i++)
            {
                var (hit, x) = (rule.Items[i], expected[i]);
                hit.N.Should().Be(x.GetProperty("n").GetInt32());
                hit.Obs.Should().Be(x.GetProperty("obs").GetString(), $"{rule.Code} item {hit.N}");
                hit.Ref.Should().Be(x.GetProperty("ref").GetString());
                if (x.TryGetProperty("operacao", out var op))
                    hit.Operation.Should().Be(op.GetString());
                if (x.TryGetProperty("chamadas", out var calls))
                    hit.Calls.Should().Equal(calls.EnumerateArray().Select(c => c.GetString()));
            }
        }
        var descriptions = e.GetProperty("regras");
        foreach (var rule in result.Rules.Where(r => r.Code is not ("R2" or "R5" or "R7" or "R8" or "R9") || true))
            rule.Description.Should().Be(descriptions.GetProperty(rule.Code).GetString());
    }

    [Fact]
    public void Aplicar_grava_status_e_obs_exatos_e_o_export_bate_byte_a_byte()
    {
        var (doc, outcome, items) = GoldenFixture.Import();
        var result = GoldenFixture.Triage(items, ProjectType.Conversion);
        var now = DateTimeOffset.UtcNow;
        foreach (var code in new[] { "R1", "R2", "R5", "R7", "R8", "MIGNOW_FSCM_PRODUCT" })
            foreach (var hit in result.Rule(code)!.Items)
                items.Single(i => i.N == hit.N).MarkStatus(TriageEngine.StatusOf(result.Rule(code)!)!, hit.Obs, now);
        var expected = GoldenFixture.Expected("sessao_aplicada.json").EnumerateArray().ToList();
        foreach (var x in expected)
        {
            var item = items.Single(i => i.N == x.GetProperty("n").GetInt32());
            item.Status.Should().Be(x.GetProperty("status").GetString());
            item.Obs.Should().Be(x.GetProperty("obs").GetString());
        }
        var session = RemediationSession.Create(1, 1, RemediationSource.CsvImport, "atc.csv", "1,2", null, "AUT-TS", ProjectType.Conversion,
            Mignow.Atc.Application.Common.Json.Write(doc.Header), doc.Encoding, doc.Delimiter, 1);
        var kept = items.Select(i => i.N).ToHashSet();
        session.SetExcludedRows(SessionExporter.ExcludedJson(doc.Rows.Select((r, k) => (N: k + 1, Row: r)).Where(x => !kept.Contains(x.N))));
        var export = SessionExporter.Export(session, items);
        export.Content.Should().Equal(File.ReadAllBytes(Path.Combine(GoldenFixture.Dir, "esperado", "atc_status.csv")));
        var summary = GoldenFixture.Expected("exportar.json").GetProperty("resumo").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32());
        export.Summary.Should().BeEquivalentTo(summary);
        _ = outcome;
    }

    [Fact]
    public async Task Grupos_batem_com_o_script()
    {
        var (_, _, items) = GoldenFixture.Import();
        var kb = Substitute.For<IKnowledgeBase>();
        kb.LookupAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ci =>
            ci.ArgAt<string>(0) == "KONV"
                ? new KbLookupResult([new KbCard("KONV-001", 5, "Troca de tabela", "SELECT * FROM konv INTO TABLE lt.", "SELECT * FROM prcd_elements INTO TABLE lt.",
                    "elementos/tabelas/KONV.md", false, [], "simples", null)], 1)
                : new KbLookupResult([], 0));
        var result = await GroupEngine.GroupAsync(items, 40, GoldenFixture.Sources(), kb, CancellationToken.None);
        var e = GoldenFixture.Expected("grupos.json");
        result.Pending.Should().Be(e.GetProperty("pendentes").GetInt32());
        result.ObjectsInBatch.Should().Be(e.GetProperty("objetos_no_lote").GetInt32());
        result.ItemsInBatch.Should().Be(e.GetProperty("itens_no_lote").GetInt32());
        var expected = e.GetProperty("grupos").EnumerateArray().ToList();
        result.Groups.Should().HaveCount(expected.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            var (g, x) = (result.Groups[i], expected[i]);
            g.Ref.Should().Be(x.GetProperty("ref").GetString());
            g.RefType.Should().Be(x.GetProperty("robt").GetString());
            g.CallForm.Should().Be(x.GetProperty("forma_chamada").GetString());
            g.Items.Should().Equal(x.GetProperty("itens").EnumerateArray().Select(n => n.GetInt32()));
            g.Objects.Should().Equal(x.GetProperty("objetos").EnumerateArray().Select(n => n.GetString()));
            g.KbSolutions.Should().Be(x.GetProperty("kb_solucoes").GetInt32());
            g.Complexity.Should().Be(x.GetProperty("complexidade").GetString());
            g.Standard.Should().Be(x.GetProperty("standard").GetBoolean());
            g.SuggestedModel.Should().Be(x.GetProperty("modelo_sugerido").GetString());
            var top = x.GetProperty("kb_top");
            if (top.ValueKind == JsonValueKind.Null)
                g.KbTop.Should().BeNull();
            else
            {
                g.KbTop!.Id.Should().Be(top.GetProperty("id").GetString());
                g.KbTop.Occurrences.Should().Be(top.GetProperty("ocorrencias").GetInt32());
            }
        }
    }
}
