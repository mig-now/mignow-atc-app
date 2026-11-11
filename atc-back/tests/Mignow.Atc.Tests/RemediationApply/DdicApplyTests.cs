using FluentAssertions;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.RemediationApply;

/// <summary>DDIC (estrutura/tabela) como o <c>aplicar_ddic.ps1</c>: aborta mudança de estrutura da tabela, grava mudança de tipo, rollback e criação.</summary>
public sealed class DdicApplyTests
{
    private static RemediationItem TableItem(string after, string name = "ZTAB")
    {
        var item = ApplyTestKit.Item(9, name, "TABL", null, null);
        item.SetProposal(new ItemProposal("IA", null, [new ProposalBlock(["  amount : abap.dec(13,2);"], [after], null, "modificacao", null)], null, null, null, [], null, null, null, null,
            "troca", "corrigir", null, ApplyTestKit.Now));
        item.SetPreparation(new ItemPreparation(name, $"ddic://TABL/{name}", [], "x", null, null, ApplyTestKit.Now));
        item.MarkStatus(RemediationStatus.ConfirmedAi, "IA", ApplyTestKit.Now);
        return item;
    }

    private static ApplyInput In(RemediationItem item, bool allow = false, Dictionary<int, DdicCreation>? create = null) =>
        new("SBXK900001", "AUT-ML", "MIGATC", [item], [], allow, create ?? []);

    [Fact]
    public async Task Mudanca_so_de_tipo_e_ativada_com_backup_e_rollback_restaura()
    {
        var kit = new ApplyTestKit();
        var item = TableItem("  amount : abap.curr(13,2);");
        var engine = kit.Apply();
        (await engine.SimulateAsync(In(item), kit.HostFor())).Reports.Single().Result.Should().Be("SIMULADO");
        (await engine.ApplyAsync(In(item), kit.HostFor())).Reports.Single().Result.Should().Be("ATIVADO");
        kit.State.WithLock(() => kit.State.GetDdic("TABL", "ZTAB")!.ActiveSource).Should().Contain("abap.curr(13,2)");
        item.Application!.BackupSource.Should().Be(ApplyTestKit.Ddl);
        (await engine.RollbackAsync(In(item), kit.HostFor())).Reports.Single().Result.Should().Be("ROLLBACK_ATIVADO");
        kit.State.WithLock(() => kit.State.GetDdic("TABL", "ZTAB")!.ActiveSource).Should().Be(ApplyTestKit.Ddl);
    }

    [Fact]
    public async Task Mudanca_de_campos_da_tabela_aborta_sem_o_aval()
    {
        var kit = new ApplyTestKit();
        var item = TableItem("  amount : abap.dec(13,2);\n  extra : abap.char(1);".Replace("\n", "\n"));
        var batch = await kit.Apply().ApplyAsync(In(item), kit.HostFor());
        batch.Reports.Single().Result.Should().Be("ABORTADO_ESTRUTURA_TABELA");
        kit.State.Journal.Should().NotContain(j => j.StartsWith("DDIC_PUT", StringComparison.Ordinal));
        (await kit.Apply().ApplyAsync(In(item, allow: true), kit.HostFor())).Reports.Single().Result.Should().Be("ATIVADO");
    }

    [Fact]
    public async Task Sem_diferenca_nada_a_fazer()
    {
        var kit = new ApplyTestKit();
        var item = TableItem("  amount : abap.dec(13,2);");
        (await kit.Apply().ApplyAsync(In(item), kit.HostFor())).Reports.Single().Result.Should().Be("NADA_A_FAZER");
    }

    [Fact]
    public async Task Erro_de_ativacao_do_ddl_e_producao_bloqueada()
    {
        var kit = new ApplyTestKit();
        var item = TableItem("  amount : abap.curr(13,2);");
        kit.State.InjectFailure("ACTIVATE", "ZTAB");
        (await kit.Apply().ApplyAsync(In(item), kit.HostFor())).Reports.Single().Result.Should().Be("ERRO_ATIVACAO");
        var blocked = await kit.Apply().ApplyAsync(In(item), kit.HostFor(production: true));
        blocked.StopCode.Should().Be("Sap.WriteBlockedProduction");
    }

    [Fact]
    public async Task Objeto_inexistente_valida_cria_e_o_rollback_fica_indisponivel()
    {
        var kit = new ApplyTestKit();
        var item = ApplyTestKit.Item(8, "ZNOVA", "STRU", null, null);
        item.SetProposal(new ItemProposal("IA", null, [new ProposalBlock(["x"], ["define structure znova {\n  a : abap.char(1);\n}"], null, "modificacao", null)], null, null, null, [], null, null, null, null,
            "cria", "corrigir", null, ApplyTestKit.Now));
        item.SetPreparation(new ItemPreparation("ZNOVA", "ddic://STRU/ZNOVA", [], "define structure znova {\n  a : abap.char(1);\n}\n", null, null, ApplyTestKit.Now));
        item.MarkStatus(RemediationStatus.ConfirmedAi, "IA", ApplyTestKit.Now);
        var engine = kit.Apply();
        (await engine.ApplyAsync(In(item), kit.HostFor())).Reports.Single().Result.Should().Be("ERRO_VALIDACAO");
        var create = new Dictionary<int, DdicCreation> { [8] = new("ZPACK", "Estrutura nova") };
        (await engine.ApplyAsync(In(item, create: create), kit.HostFor())).Reports.Single().Result.Should().Be("ATIVADO");
        (await engine.RollbackAsync(In(item), kit.HostFor())).Reports.Single().Result.Should().Be("ROLLBACK_INDISPONIVEL");
    }
}
