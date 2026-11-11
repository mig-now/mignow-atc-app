using FluentAssertions;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.RemediationApply;

public sealed class ApplyEngineTests
{
    private static async Task<(ApplyTestKit Kit, RemediationItem Item, SourceCache Cache)> Prepared()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S00012", "simples");
        var item = ApplyTestKit.Item(1);
        var cache = kit.Cache();
        await kit.Propose().RunAsync(kit.Input("kb", [item], [cache]), kit.HostFor());
        item.MarkStatus(RemediationStatus.ConfirmedKb, "KB S00012", ApplyTestKit.Now);
        return (kit, item, cache);
    }

    private static ApplyInput In(RemediationItem item, SourceCache cache, string? request = "SBXK900001") =>
        new(request, "AUT-ML", "MIGATC", [item], [cache], false, new Dictionary<int, DdicCreation>());

    [Fact]
    public async Task Simulacao_guarda_o_backup_e_nao_grava_no_sap()
    {
        var (kit, item, cache) = await Prepared();
        var batch = await kit.Apply().SimulateAsync(In(item, cache), kit.HostFor());
        batch.Reports.Single().Result.Should().Be("SIMULADO");
        item.Application!.BackupSource.Should().Be(ApplyTestKit.Source);
        kit.State.Journal.Should().NotContain(j => j.StartsWith("PUT", StringComparison.Ordinal) || j.StartsWith("LOCK", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Fonte_ativo_diferente_do_lido_aborta()
    {
        var (kit, item, cache) = await Prepared();
        kit.State.WithLock(() => kit.State.Get("ZPROG")!.ActiveSource = ApplyTestKit.Source + "* alteração de terceiros\n");
        var batch = await kit.Apply().SimulateAsync(In(item, cache), kit.HostFor());
        batch.Reports.Single().Result.Should().Be("ABORTADO_FONTE_MUDOU");
    }

    [Fact]
    public async Task Aplica_com_lock_put_unlock_ativacao_e_atualiza_o_cache()
    {
        var (kit, item, cache) = await Prepared();
        var batch = await kit.Apply().ApplyAsync(In(item, cache), kit.HostFor());
        batch.Reports.Single().Result.Should().Be("ATIVADO");
        item.Application!.Result.Should().Be("ATIVADO");
        item.Application.Request.Should().Be("SBXK900001");
        item.Application.History.Should().ContainSingle().Which.Mode.Should().Be("aplicacao");
        kit.State.Journal.Where(j => !j.StartsWith("SQL", StringComparison.Ordinal)).Should().Contain(["LOCK ZPROG", "PUT ZPROG SBXK900001", "UNLOCK ZPROG", "ACTIVATE ZPROG"]);
        kit.State.WithLock(() => kit.State.Get("ZPROG")!.ActiveSource).Should().Be(item.Preparation!.NewSource);
        cache.Source.Should().Be(item.Preparation.NewSource);
    }

    [Fact]
    public async Task Item_sem_confirmacao_nao_e_aplicado()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S1", "simples");
        var item = ApplyTestKit.Item(1);
        var cache = kit.Cache();
        await kit.Propose().RunAsync(kit.Input("kb", [item], [cache]), kit.HostFor());
        var batch = await kit.Apply().ApplyAsync(In(item, cache), kit.HostFor());
        batch.Reports.Single().Result.Should().Be("NAO_PRONTO");
        kit.State.Journal.Should().NotContain(j => j.StartsWith("PUT", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Sem_request_a_aplicacao_para()
    {
        var (kit, item, cache) = await Prepared();
        var batch = await kit.Apply().ApplyAsync(In(item, cache, null), kit.HostFor());
        batch.StopCode.Should().Be("Apply.RequestRequired");
    }

    [Fact]
    public async Task Erro_de_ativacao_deixa_inativo_e_o_rollback_restaura_o_backup()
    {
        var (kit, item, cache) = await Prepared();
        var engine = kit.Apply();
        await engine.SimulateAsync(In(item, cache), kit.HostFor());
        kit.State.InjectFailure("ACTIVATE", "ZPROG");
        (await engine.ApplyAsync(In(item, cache), kit.HostFor())).Reports.Single().Result.Should().Be("ERRO_ATIVACAO");
        kit.State.WithLock(() => kit.State.Get("ZPROG")!.ActiveSource).Should().Be(ApplyTestKit.Source);
        item.Application!.BackupSource.Should().Be(ApplyTestKit.Source);

        var rollback = await engine.RollbackAsync(In(item, cache), kit.HostFor());
        rollback.Reports.Single().Result.Should().Be("ROLLBACK_ATIVADO");
        item.Application.ErrorDecision.Should().Be("rollback");
        kit.State.WithLock(() => kit.State.Get("ZPROG")!.ActiveSource).Should().Be(ApplyTestKit.Source);
    }

    [Fact]
    public async Task Falha_de_gravacao_e_lock_de_outro_usuario_e_request_diferente()
    {
        var (kit, item, cache) = await Prepared();
        kit.State.InjectFailure("PUT", "ZPROG");
        (await kit.Apply().ApplyAsync(In(item, cache), kit.HostFor())).Reports.Single().Result.Should().Be("ERRO_GRAVACAO");
        kit.State.WithLock(() => kit.State.Get("ZPROG")!.LockedBy = "OUTRO");
        (await kit.Apply().ApplyAsync(In(item, cache), kit.HostFor())).Reports.Single().Result.Should().Be("ERRO_LOCK");
        kit.State.WithLock(() => { var o = kit.State.Get("ZPROG")!; o.LockedBy = null; o.OpenRequest = "SBXK999999"; return true; });
        (await kit.Apply().ApplyAsync(In(item, cache), kit.HostFor())).Reports.Single().Result.Should().Be("ERRO_REQUEST");
    }

    [Fact]
    public async Task Producao_bloqueia_e_nada_e_gravado()
    {
        var (kit, item, cache) = await Prepared();
        var batch = await kit.Apply().ApplyAsync(In(item, cache), kit.HostFor(production: true));
        batch.StopCode.Should().Be("Sap.WriteBlockedProduction");
        item.Application!.Result.Should().Be("ERRO");
        kit.State.Journal.Should().NotContain(j => j.StartsWith("PUT", StringComparison.Ordinal) || j.StartsWith("LOCK", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Mesmo_bloco_ja_aplicado_nao_grava_de_novo()
    {
        var (kit, item, cache) = await Prepared();
        var second = ApplyTestKit.Item(2);
        second.SetProposal(item.Proposal);
        second.SetPreparation(item.Preparation);
        second.MarkStatus(RemediationStatus.ConfirmedKb, "KB", ApplyTestKit.Now);
        var input = new ApplyInput("SBXK900001", "AUT-ML", "MIGATC", [item, second], [cache], false, new Dictionary<int, DdicCreation>());
        var batch = await kit.Apply().ApplyAsync(input, kit.HostFor());
        batch.Reports.Select(r => r.Result).Should().Equal("ATIVADO", "ATIVADO");
        kit.State.Journal.Count(j => j.StartsWith("PUT", StringComparison.Ordinal)).Should().Be(1);
    }
}
