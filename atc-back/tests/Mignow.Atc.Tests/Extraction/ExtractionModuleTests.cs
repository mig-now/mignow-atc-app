using FluentAssertions;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Ports.Sap.Extraction;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Tests.Extraction;

public sealed class ExtractionModuleTests
{
    private static AtcRunHeader H(string id, string series, string ts, bool complete = true, bool reworked = false, string? exec = null) =>
        new(id, exec ?? id, "t", series, ts, "U", complete, reworked, null, null, 1, 2, 3, 0, 0);

    [Fact]
    public void Seleciona_a_ultima_concluida_da_serie_exata_e_informa_as_mais_novas_em_andamento()
    {
        var headers = new[]
        {
            H("A", "ZMIG_X", "20261001000000.0"), H("B", "Z_MIG_Y", "20261010000000.0"), H("C", "ZMIGRA", "20261020000000.0"),
            H("D", "ZMIG_X", "20261015000000.0", reworked: true, exec: "B"), H("E", "ZMIG_X", "20261012000000.0", exec: "OUTRO"),
            H("F", "ZMIG_Z", "20261030000000.0", complete: false), H("G", "ZMIG_Z", "20260901000000.0", complete: false),
        };
        var s = AtcRunSelector.Select(headers, AtcRunSelector.DefaultPrefixes);
        s.Latest!.DisplayId.Should().Be("B");
        s.InProgress.Select(h => h.DisplayId).Should().Equal("F");
        s.ExecutionsFound.Should().Be(4);
        AtcRunSelector.Select([H("A", "OUTRA", "1")], AtcRunSelector.DefaultPrefixes).Latest.Should().BeNull();
    }

    [Fact]
    public void Nomes_de_arquivo_seguem_a_skill()
    {
        var b = ExtractionNaming.BaseName("DI4", "400", "Z_MIG ATC/1", AtcRunSelector.ParseUtc("20261020123456.1234567"), "ABC123");
        b.Should().Be("atc_DI4_400_Z_MIG_ATC_1_20261020-123456_ABC123");
        ExtractionNaming.FileName(b, ExtractionFileKind.New).Should().EndWith("_ABC123_novos.csv");
        ExtractionNaming.FileName(b, ExtractionFileKind.Remediation).Should().EndWith("_ABC123_remediacao.csv");
        ExtractionFiles.FileName(b, ExtractionFileKind.Total, [1, 2]).Should().EndWith("_ABC123_p1-2.csv");
        ExtractionFiles.ParsePriorities("2, 1").Should().Equal(1, 2);
        ExtractionFiles.ParsePriorities("4").Should().BeNull();
    }

    [Fact]
    public void Sem_tabela_de_baseline_todos_os_findings_sao_novos_e_a_ausencia_e_informada()
    {
        // MI9 (S4CORE 108 SP01): SATC_AC_BSI não existe; a extração segue sem baseline em vez de falhar
        var scenario = new SapScenario
        {
            Objects = [new ScenarioObject("ZMIG_A", "PROG/P", "ZPKG", "REPORT zmig_a.\nSELECT * FROM bseg INTO TABLE lt.\nSELECT * FROM mkpf INTO TABLE l2.\n")],
            AtcRules = [new ScenarioRule("select * from", "No star", Check: "Perf", Priority: 3)],
            AtcRuns = [new ScenarioAtcRun("RUN1", "ATC sim", new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero))],
            AtcExtraction = new ScenarioExtraction(BaselineEvery: 1, MissingTables: ["SATC_AC_BSI"]),
        };
        var state = new SimulatedSapState(scenario);
        var complements = state.AtcComplementsFor("RUN1");
        complements.MissingTables.Should().Equal("SATC_AC_BSI");
        complements.Baselines.Should().BeEmpty();
        complements.NoteTitles.Should().NotBeEmpty();
        var counts = ExtractionCounts.Of(ExtractionTableBuilder.Build(state.AtcFindings("RUN1")!, complements).Rows);
        counts.Total.Should().Be(2);
        counts.InBaseline.Should().Be(0);
        counts.New.Should().Be(2);
    }

    [Fact]
    public async Task Simulador_gera_execucao_coerente_e_a_tabela_sai_na_ordem_das_colunas()
    {
        var scenario = new SapScenario
        {
            Objects = [new ScenarioObject("ZMIG_A", "PROG/P", "ZPKG", "REPORT zmig_a.\nSELECT * FROM bseg INTO TABLE lt.\n* from bseg\nSELECT * FROM mkpf INTO TABLE l2.\n")],
            AtcRules =
            [
                new ScenarioRule("from bseg", "BSEG replaced", Check: "S4 Readiness", Priority: 2),
                new ScenarioRule("select * from", "No star", Check: "Perf", Priority: 3),
            ],
            AtcRuns = [new ScenarioAtcRun("RUN1", "ATC sim", new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero))],
            AtcExtraction = new ScenarioExtraction(BaselineEvery: 2, InProgressRun: true),
        };
        var state = new SimulatedSapState(scenario);
        var raw = state.AtcFindings("RUN1")!;
        raw.Rows.Should().HaveCount(3); // from bseg (linha 2) + select * from (linhas 2 e 4); o comentário não conta
        var table = ExtractionTableBuilder.Build(raw, state.AtcComplementsFor("RUN1"));
        table.Columns.Should().StartWith("obj_uri", "obj_type", "obj_name", "obj_packageName", "tadir_author", "tadir_devclass", "uri");
        table.Columns.Should().ContainInOrder("item_id", "documentation_link", "exemption_kind_text", "tag_ADD_INFO", "change_category_text", "note_short_text",
            "itm_type", "baseline_ix", "baseline_effect", "baseline_effect_text");
        var counts = ExtractionCounts.Of(table.Rows);
        counts.Total.Should().Be(3);
        counts.InBaseline.Should().Be(1);
        counts.New.Should().Be(2);
        (counts.P2Total + counts.P3Total).Should().Be(3);
        table.Rows[0]["tadir_author"].Should().Be("DEVELOPER");
        table.Rows[1]["baseline_effect_text"].Should().Be("Exempted by baseline");
        ExtractionQuality.Of(table.Rows).WithoutItem.Should().Be(0);
        var sel = AtcRunSelector.Select(state.AtcHeaders(AtcRunSelector.DefaultPrefixes), AtcRunSelector.DefaultPrefixes);
        sel.Latest!.DisplayId.Should().Be("RUN1");
        sel.InProgress.Should().ContainSingle();
    }

    [Fact]
    public void Variacao_compara_com_a_extracao_anterior()
    {
        var prev = AtcExtraction.Create(1, "ZMIG_X", "OLD", null, null, DateTimeOffset.UtcNow);
        prev.SetCounts(10, 4, 1, 2, 7, 1, 1, 3);
        var v = ExtractionVariation.Compute(new ExtractionCounts(12, 5, 2, 2, 8, 1, 0, 6), prev);
        v.Total.Should().Be(2);
        v.New.Should().Be(1); // 7 novos contra 6
        v.NewByPriority.Should().Be(new PriorityVariation(0, -1, 3));
    }
}
