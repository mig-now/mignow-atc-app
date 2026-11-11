using FluentAssertions;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Sap.Scan;

namespace Mignow.Atc.Tests.Scan;

/// <summary>Inventário (inventario.ps1), seleção/rodízio de lotes (executar_paralelo.ps1) e leitura da saída da classe.</summary>
public sealed class ScanLogicTests
{
    private static ScanTadirRow Row(string type, string name, string package = "ZPKG", string gen = "", string del = "") => new(type, name, package, "AUT", gen, del);

    [Fact]
    public void Inventario_exclui_tmp_marcados_e_gerados_e_ordena_por_tipo_e_nome()
    {
        var rows = new[]
        {
            Row("PROG", "ZB"), Row("PROG", "ZA"), Row("CLAS", "ZCL_X"), Row("FUGR", "ZFG"), Row("INTF", "ZIF"),
            Row("PROG", "ZTMP", "$TMP"), Row("PROG", "ZDEL", del: "X"), Row("PROG", "ZGEN", gen: "X"),
        };
        var inv = ScanInventoryBuilder.Build(rows, false, 1000, ScanInventoryBuilder.DefaultTypes, "ZCL_MIGNOW_ATC_SCAN");
        inv.TadirRead.Should().Be(8);
        (inv.ExcludedTmp, inv.ExcludedDeleted, inv.ExcludedGenerated).Should().Be((1, 1, 1));
        inv.Batches.Should().ContainSingle().Which.Objects.Select(o => $"{o.Type} {o.Name}")
            .Should().Equal("CLAS ZCL_X", "FUGR ZFG", "INTF ZIF", "PROG ZA", "PROG ZB");
        inv.Batches[0].Weight.Should().Be(3 + 5 + 1 + 1 + 1);
        inv.ByType.Should().Equal(new Dictionary<string, int> { ["CLAS"] = 1, ["FUGR"] = 1, ["INTF"] = 1, ["PROG"] = 2 });
        ScanInventoryBuilder.Build(rows, true, 1000, ScanInventoryBuilder.DefaultTypes, "Z").ScopeCount.Should().Be(6);
    }

    [Fact]
    public void Lotes_por_peso_abrem_novo_lote_quando_estoura()
    {
        var rows = new[] { Row("CLAS", "ZA"), Row("CLAS", "ZB"), Row("CLAS", "ZC"), Row("PROG", "ZP") };
        var inv = ScanInventoryBuilder.Build(rows, false, 6, ScanInventoryBuilder.DefaultTypes, "Z");
        inv.Batches.Select(b => (b.Number, b.Weight, b.Objects.Count)).Should().Equal((1, 6, 2), (2, 4, 2));
    }

    [Fact]
    public void Selecao_de_lotes_e_rodizio()
    {
        ScanProcessPlanner.ParseLots("2-4,7", [1, 2, 3, 4, 5, 6]).Should().Equal(2, 3, 4);
        ScanProcessPlanner.ParseLots(null, [3, 1, 2]).Should().Equal(1, 2, 3);
        var groups = ScanProcessPlanner.RoundRobin([2, 3, 4, 5, 6], 3);
        groups.Select(g => string.Join(',', g)).Should().Equal("2,5", "3,6", "4");
        ScanProcessPlanner.RoundRobin([1, 2], 9).Should().HaveCount(2);
        ScanProcessPlanner.AllClasses("ZCL_MIGNOW_ATC_SCAN", 2).Should().Equal("ZCL_MIGNOW_ATC_SCAN", "ZCL_MIGNOW_ATC_SCAN_1", "ZCL_MIGNOW_ATC_SCAN_2");
        ScanProcessPlanner.ValidClassName("ZCL_MIGNOW_ATC_SCAN_1").Should().BeTrue();
        ScanProcessPlanner.ValidClassName("CL_X").Should().BeFalse();
    }

    [Fact]
    public void Saida_do_lote_e_interpretada()
    {
        var text = "L\t001\t2\nH\tPROG\tZP\tprograms/programs/zp\t3\t4\tREGEX_001\tVBUK\tP\ta\tb\nF\tPROG\tZP\t1\t10\nE\tPROG\tZX\tfalha\n" +
                   "O\tPROG\tZP\tprograms/programs/zp\tREGEX_001\t2\nX\tREGEX_009\tbad\nZ\t001\t2\t55\t1\n";
        var o = ScanOutputParser.Parse(text);
        o.Complete.Should().BeTrue();
        o.Hits.Should().ContainSingle().Which.Should().Be(new ScanHit("PROG", "ZP", "programs/programs/zp", 3, 4, "REGEX_001", "VBUK", "P", "a\tb"));
        var stats = o.ToStats(1, null, [], null);
        (stats.ObjectsRead, stats.SourcesRead, stats.LinesRead, stats.ObjectErrors, stats.RegexErrors, stats.ServerMs, stats.ScanErrors).Should().Be((1, 1, 10, 1, 1, 55, 1));
        stats.OriginalByRule.Should().Equal(new Dictionary<string, int> { ["REGEX_001"] = 2 });
        ScanOutputParser.Parse("L\t001\t2\n").Complete.Should().BeFalse();
    }

    [Fact]
    public void Regras_ativas_so_mudam_o_campo_ativa()
    {
        var json = """{"versao":"1","gatilho":"A","regras":[{"id":"R1","refinada":{"pcre":"x"}},{"id":"R2","refinada":{"pcre":"y"}}]}""";
        var changed = MignowRulesDocument.WithActive(json, ["R2"]);
        MignowRulesDocument.Parse(changed).ActiveRules.Select(r => r.Id).Should().Equal("R2");
        MignowRulesDocument.Parse(json).ActiveRules.Should().HaveCount(2);
    }
}
