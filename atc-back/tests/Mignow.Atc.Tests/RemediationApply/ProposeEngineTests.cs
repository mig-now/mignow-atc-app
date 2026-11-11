using FluentAssertions;
using NSubstitute;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.RemediationApply;

public sealed class ProposeEngineTests
{
    [Fact]
    public async Task Modo_kb_simples_usa_mechanical_e_monta_bloco_diff_e_sintaxe()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S00012", "simples");
        var item = ApplyTestKit.Item(1);
        var summary = await kit.Propose().RunAsync(kit.Input("kb", [item], [kit.Cache()]), kit.HostFor());

        summary.Fix.Should().Be(1);
        summary.FromKb.Should().Be(1);
        kit.Usages.Should().ContainSingle().Which.Purpose.Should().Be(AiUsage.Mechanical);
        var p = item.Proposal!;
        p.Origin.Should().Be("KB");
        p.KbCardId.Should().Be("S00012");
        p.Action.Should().Be("corrigir");
        p.NewSource.Should().Contain("*>>> S/4 MIGRATION MIG-NOW - START OF DELETION - 06.11.2026 AUT-ML").And.Contain("*SELECT * FROM konv");
        p.Diff.Should().StartWith("--- ZPROG (atual)");
        p.SyntaxOk.Should().BeTrue();
        item.Preparation!.NewSource.Should().Be(p.NewSource);
        item.Preparation.Ranges.Should().ContainSingle().Which.Start.Should().Be(3);
        p.MarkedBlock.Should().Contain("END OF INSERTION");
    }

    [Fact]
    public async Task Cartao_com_alerta_vira_analise_da_ia_com_modelo_analysis()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S1", "complexa", "alerta da KB");
        var item = ApplyTestKit.Item(1);
        await kit.Propose().RunAsync(kit.Input("kb", [item], [kit.Cache()]), kit.HostFor());
        kit.Usages.Single().Purpose.Should().Be(AiUsage.Analysis);
        item.Proposal!.Origin.Should().Be("IA");
        item.Proposal.KbCardId.Should().BeNull();
    }

    [Fact]
    public async Task Modo_ia_desconsidera_a_kb_e_usa_analysis()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S1", "simples");
        var item = ApplyTestKit.Item(1);
        await kit.Propose().RunAsync(kit.Input("ia", [item], [kit.Cache()]), kit.HostFor());
        kit.Usages.Single().Purpose.Should().Be(AiUsage.Analysis);
        item.Proposal!.Origin.Should().Be("IA");
        item.Proposal.Action.Should().Be("corrigir");
    }

    [Fact]
    public async Task Media_usa_review_e_itens_no_mesmo_trecho_viram_mesmo_bloco()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S2", "media");
        RemediationItem[] items = [ApplyTestKit.Item(1), ApplyTestKit.Item(2)];
        await kit.Propose().RunAsync(kit.Input("kb", items, [kit.Cache()]), kit.HostFor());
        kit.Usages.Should().ContainSingle().Which.Purpose.Should().Be(AiUsage.Review);
        items[1].Proposal!.Action.Should().Be("corrigir");
        items[1].Preparation!.NewSource.Should().Be(items[0].Preparation!.NewSource);
        items[1].Proposal!.Justification.Should().Contain("Apply.Propose.SameBlock");
    }

    [Fact]
    public async Task Modo_kb_sem_cartao_fica_pendente_sem_chamar_a_ia()
    {
        var kit = new ApplyTestKit();
        kit.Kb.LookupAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(new KbLookupResult([], 0));
        var item = ApplyTestKit.Item(1);
        var summary = await kit.Propose().RunAsync(kit.Input("kb", [item], [kit.Cache()]), kit.HostFor());
        summary.Pending.Should().Be(1);
        kit.Usages.Should().BeEmpty();
        item.Proposal!.Action.Should().Be("pendente");
    }

    [Fact]
    public async Task Item_sem_fonte_lido_fica_pendente()
    {
        var kit = new ApplyTestKit();
        var item = ApplyTestKit.Item(1, "ZOUTRO");
        await kit.Propose().RunAsync(kit.Input("ia", [item], [kit.Cache()]), kit.HostFor());
        item.Proposal!.Action.Should().Be("pendente");
        item.Preparation.Should().BeNull();
    }

    [Fact]
    public async Task Erro_de_sintaxe_no_fonte_novo_marca_syntaxOk_falso()
    {
        var kit = new ApplyTestKit();
        kit.SetKbCard("S3", "simples");
        var item = ApplyTestKit.Item(1);
        var source = ApplyTestKit.Source.Replace("WRITE 'x'.", "BAD_STATEMENT.\nSELECT * FROM konv INTO TABLE lt_konv WHERE x = 1.");
        // a linha BAD_STATEMENT já existe no fonte lido: o fonte novo também a tem
        await kit.Propose().RunAsync(kit.Input("kb", [item], [kit.Cache(source)]), kit.HostFor());
        item.Proposal!.SyntaxOk.Should().BeFalse();
        item.Proposal.SyntaxMessages.Should().Contain(m => m.Contains("Comando inválido"));
    }
}
