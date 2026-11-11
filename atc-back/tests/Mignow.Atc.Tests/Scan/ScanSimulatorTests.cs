using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Scan;
using Mignow.Atc.Infra.External.Sap.Simulator;
using NSubstitute;

namespace Mignow.Atc.Tests.Scan;

/// <summary>Porta IMignowScanner no simulador: inventário, simulação, execução (cria, roda, apaga) e resíduos, com a trava de produção.</summary>
public sealed class ScanSimulatorTests
{
    private const string Vbuk = "REPORT zp.\nSELECT * FROM vbuk INTO TABLE lt WHERE vbeln = '1'.\n* SELECT * FROM konv.\nSELECT * FROM konv\n  INTO TABLE lt2.\n";

    private static (SimulatedSap Sap, SimulatedSapState State, SapConnection Conn) Make(string category = "T", bool production = false)
    {
        var scenario = new SapScenario
        {
            ClientCategory = category,
            Objects =
            [
                new ScenarioObject("ZPROG_A", "PROG/P", "ZPKG", Vbuk),
                new ScenarioObject("ZPROG_CLEAN", "PROG/P", "ZPKG", "REPORT zc.\nWRITE 'ok'.\n"),
                new ScenarioObject("ZPROG_TMP", "PROG/P", "$TMP", Vbuk),
                new ScenarioObject("RV_STD", "PROG/P", "SAPSTD", Vbuk),
                new ScenarioObject("ZFG", "FUGR/F", "ZPKG", "FUNCTION-POOL zfg."),
                new ScenarioObject("ZFM", "FUGR/FF", "ZPKG", "FUNCTION zfm.\n  SELECT * FROM vbuk INTO TABLE lt.\nENDFUNCTION.", MainProgram: "ZFG"),
            ],
        };
        var env = Substitute.For<IHostEnvironment>();
        var registry = new SimulatedSapRegistry(Options.Create(new SapSimulatorOptions()), env);
        var state = registry.Register("scantest", scenario);
        var l = Substitute.For<IStringLocalizer<Errors>>();
        l[Arg.Any<string>(), Arg.Any<object[]>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        l[Arg.Any<string>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        return (new SimulatedSap(registry, l), state, new SapConnection(Guid.NewGuid(), "sim://scantest", "100", "U", string.Empty, "EN", null, production));
    }

    private static ScanLotRequest Lot(int n, params (string Type, string Name)[] objects)
    {
        var doc = MignowRulesDocument.Parse("""
            {"versao":"1","gatilho":"(?i)VBUK|KONV","regras":[
             {"id":"REGEX_001","ativa":true,"origem":{"pcre":".*(FROM|JOIN)[[:space:]]+(VBUK|VBUP).*"},"refinada":{"pcre":"\\b(?:FROM|JOIN)\\s+\\(*\\s*(VBUK|VBUP)\\b","texto":"C"}},
             {"id":"REGEX_002","ativa":true,"origem":{"pcre":".*FROM[[:space:]]+KONV.*"},"refinada":{"pcre":"\\b(?:FROM|JOIN)\\s+\\(*\\s*(KONV)\\b","texto":"C"}}]}
            """);
        var specs = doc.Rules.Select(r => new ScanRuleSpec(r.Id, r.Pcre, r.Text, r.FixedElement, r.Requires, r.SuppressedBy, r.OriginalPcre)).ToList();
        return new ScanLotRequest(n, "ABAP", [.. objects.Select(o => new ScanLotObject(o.Type, o.Name))], doc.Trigger, specs, true);
    }

    [Fact]
    public async Task Inventario_filtra_por_nome_ou_pacote_zy_e_tipos()
    {
        var (sap, _, conn) = Make();
        var rows = (await sap.InventoryAsync(conn, ScanInventoryBuilder.DefaultTypes, default)).Value;
        rows.Select(r => r.Name).Should().BeEquivalentTo("ZPROG_A", "ZPROG_CLEAN", "ZPROG_TMP", "ZFG");
        rows.Should().NotContain(r => r.Name == "RV_STD" || r.Name == "ZFM");
    }

    [Fact]
    public async Task Executa_o_lote_com_a_saida_do_template_e_apaga_a_classe()
    {
        var (sap, state, conn) = Make();
        var results = new List<ScanLotResult>();
        var r = await sap.ExecuteProcessAsync(conn, new ScanProcessRequest("ZCL_MIGNOW_ATC_SCAN_1", [Lot(1, ("PROG", "ZPROG_A"), ("PROG", "ZPROG_CLEAN"), ("FUGR", "ZFG"))]),
            res =>
            {
                results.Add(res);
                return Task.FromResult(true);
            }, default);

        r.Value.Created.Should().BeTrue();
        r.Value.Removal.Removed.Should().BeTrue();
        state.TemporaryClasses.Should().BeEmpty();
        state.Journal.Should().Contain(["SCAN_CLASS_CREATE ZCL_MIGNOW_ATC_SCAN_1", "SCAN_CLASS_DELETE ZCL_MIGNOW_ATC_SCAN_1"]);
        var parsed = ScanOutputParser.Parse(results.Single().Output);
        parsed.Complete.Should().BeTrue(results.Single().Output);
        parsed.Hits.Select(h => $"{h.Name}|{h.Line}|{h.RuleId}|{h.Element}|{h.Flag}").Should().BeEquivalentTo(
            ["ZPROG_A|2|REGEX_001|VBUK|P", "ZPROG_A|4|REGEX_002|KONV|P", "ZFG|2|REGEX_001|VBUK|P"], results.Single().Output);
        parsed.Hits.Single(h => h.Name == "ZFG").Source.Should().Be("functions/groups/zfm".Replace("zfm", "zfg/fmodules/zfm"));
        parsed.Hits.First(h => h.RuleId == "REGEX_002").LineEnd.Should().Be(5);
        parsed.Read.Should().HaveCount(3);
        parsed.Original.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Falha_de_ativacao_para_o_processo_mas_a_classe_e_apagada()
    {
        var (sap, state, conn) = Make();
        state.InjectFailure("SCAN_ACTIVATE", "ZCL_MIGNOW_ATC_SCAN_1");
        var seen = new List<string>();
        var r = await sap.ExecuteProcessAsync(conn, new ScanProcessRequest("ZCL_MIGNOW_ATC_SCAN_1", [Lot(1, ("PROG", "ZPROG_A")), Lot(2, ("PROG", "ZPROG_CLEAN"))]),
            res =>
            {
                seen.Add($"{res.Number}:{res.Status}");
                return Task.FromResult(true);
            }, default);
        seen.Should().Equal("1:" + ScanBatchStatus.ActivationError);
        r.Value.Removal.Removed.Should().BeTrue();
    }

    [Fact]
    public async Task Classe_que_nao_foi_removida_e_apontada_e_a_limpeza_remove()
    {
        var (sap, state, conn) = Make();
        state.InjectFailure("SCAN_REMOVE", "ZCL_MIGNOW_ATC_SCAN_1");
        var r = await sap.ExecuteProcessAsync(conn, new ScanProcessRequest("ZCL_MIGNOW_ATC_SCAN_1", [Lot(1, ("PROG", "ZPROG_CLEAN"))]), _ => Task.FromResult(true), default);
        r.Value.Removal.Removed.Should().BeFalse();
        r.Value.Removal.Situation.Should().StartWith("NÃO REMOVIDA");

        var sim = (await sap.SimulateAsync(conn, ["ZCL_MIGNOW_ATC_SCAN_1", "ZCL_MIGNOW_ATC_SCAN_2", "CL_BAD"], default)).Value;
        sim.ClientOk.Should().BeTrue();
        sim.Classes.Select(c => (c.ClassName, c.Exists, c.ValidationOk)).Should().Equal(("ZCL_MIGNOW_ATC_SCAN_1", true, false), ("ZCL_MIGNOW_ATC_SCAN_2", false, true), ("CL_BAD", false, false));

        var cleanup = (await sap.RemoveResiduesAsync(conn, ["ZCL_MIGNOW_ATC_SCAN", "ZCL_MIGNOW_ATC_SCAN_1"], default)).Value;
        cleanup.Select(c => (c.ClassName, c.Removed, c.Situation)).Should().Equal(("ZCL_MIGNOW_ATC_SCAN", true, "nao_existia"), ("ZCL_MIGNOW_ATC_SCAN_1", true, "removida"));
        state.TemporaryClasses.Should().BeEmpty();
    }

    [Fact]
    public async Task Residuo_fora_de_tmp_nao_e_apagado()
    {
        var (sap, state, conn) = Make();
        state.SeedTemporaryClass("ZCL_MIGNOW_ATC_SCAN", "ZPKG");
        var cleanup = (await sap.RemoveResiduesAsync(conn, ["ZCL_MIGNOW_ATC_SCAN"], default)).Value.Single();
        cleanup.Removed.Should().BeFalse();
        cleanup.Situation.Should().Contain("não é a temporária");
    }

    [Theory]
    [InlineData("P", false)]
    [InlineData("T", true)]
    public async Task Escrita_bloqueada_em_producao_e_a_simulacao_acusa(string category, bool production)
    {
        var (sap, state, conn) = Make(category, production);
        (await sap.SimulateAsync(conn, ["ZCL_MIGNOW_ATC_SCAN_1"], default)).Value.ClientOk.Should().BeFalse();
        var r = await sap.ExecuteProcessAsync(conn, new ScanProcessRequest("ZCL_MIGNOW_ATC_SCAN_1", [Lot(1, ("PROG", "ZPROG_A"))]), _ => Task.FromResult(true), default);
        r.IsFailure.Should().BeTrue();
        r.Error.Code.Should().StartWith("Sap.WriteBlocked");
        (await sap.RemoveResiduesAsync(conn, ["ZCL_MIGNOW_ATC_SCAN"], default)).IsFailure.Should().BeTrue();
        state.TemporaryClasses.Should().BeEmpty();
    }
}
