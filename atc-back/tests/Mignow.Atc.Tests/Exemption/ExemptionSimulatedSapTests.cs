using FluentAssertions;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Tests.Exemption;

/// <summary>Porta <see cref="IAtcExemptions"/> no simulador: política, modelo, escopo, envio, trava de escrita.</summary>
public sealed class ExemptionSimulatedSapTests
{
    private static ExemptionApply Apply(string marker = ExemptionSapHarness.Normal, string approver = "APPROVER1", string reason = "FPOS") =>
        new(marker, approver, reason, "FPOS - POS - ATC - MIGNOW | S/4 MIGRATION MIG-NOW 08.11.2026 AUT-TS: ok", "U");

    [Fact]
    public async Task Simulacao_le_o_modelo_e_monta_o_pedido_sem_gravar()
    {
        var h = new ExemptionSapHarness();
        var normal = (await h.Sap.PreviewAsync(h.Connection, Apply(), CancellationToken.None)).Value;
        normal.Status.Should().Be(ExemptionRequestStatus.Simulated);
        normal.RequestXml.Should().Contain("<atcexmpt:approver>APPROVER1</atcexmpt:approver>");
        (await h.Sap.PreviewAsync(h.Connection, Apply(ExemptionSapHarness.NoScope), CancellationToken.None)).Value.Status.Should().Be(ExemptionRequestStatus.NoScope);
        var syntax = (await h.Sap.PreviewAsync(h.Connection, Apply(ExemptionSapHarness.Syntax), CancellationToken.None)).Value;
        (syntax.Status, syntax.ScopeReason).Should().Be((ExemptionRequestStatus.NoScope, "finding sem checksum (-1)"));
        var blocked = (await h.Sap.PreviewAsync(h.Connection, Apply(ExemptionSapHarness.Blocked), CancellationToken.None)).Value;
        (blocked.Status, blocked.Message).Should().Be((ExemptionRequestStatus.Blocked, "Exemption already requested"));
        h.Journal.Should().NotContain(j => j.StartsWith("EXEMPTION_POST", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Envio_cria_o_pedido_e_o_modelo_passa_a_dizer_que_ja_foi_pedido()
    {
        var h = new ExemptionSapHarness();
        var sent = (await h.Sap.SubmitAsync(h.Connection, Apply(), CancellationToken.None)).Value;
        (sent.Status, sent.Http, sent.SapType).Should().Be((ExemptionRequestStatus.Requested, 200, "S"));
        sent.ResponseXml.Should().Contain("<atcexmpt:type>S</atcexmpt:type>");
        h.State.Exemptions[ExemptionSapHarness.Normal].Approver.Should().Be("APPROVER1");

        var after = (await h.Sap.GetTemplateAsync(h.Connection, ExemptionSapHarness.Normal, CancellationToken.None)).Value;
        (after.HasProposal, after.Message).Should().Be((false, SimulatedSap.AlreadyRequestedMessage));
        (await h.Sap.GetUserTotalsAsync(h.Connection, CancellationToken.None)).Value.Totals["countTotal"].Should().Be(1);
    }

    [Fact]
    public async Task Aprovador_igual_ao_solicitante_e_recusado_pelo_sap()
    {
        var h = new ExemptionSapHarness();
        var sent = (await h.Sap.SubmitAsync(h.Connection, Apply(approver: "migatc"), CancellationToken.None)).Value;
        (sent.Status, sent.SapType).Should().Be((ExemptionRequestStatus.Error, "E"));
        h.State.Exemptions.Should().BeEmpty();
    }

    [Fact]
    public async Task Falha_injetada_no_post_vira_ERRO()
    {
        var h = new ExemptionSapHarness();
        h.State.InjectFailure("EXEMPT_POST", ExemptionSapHarness.Normal);
        var sent = (await h.Sap.SubmitAsync(h.Connection, Apply(), CancellationToken.None)).Value;
        (sent.Status, sent.Http, sent.Message).Should().Be((ExemptionRequestStatus.Error, 500, "falha simulada"));
    }

    [Fact]
    public async Task Producao_no_cadastro_bloqueia_o_envio_antes_de_qualquer_chamada()
    {
        var h = new ExemptionSapHarness(production: true);
        var r = await h.Sap.SubmitAsync(h.Connection, Apply(), CancellationToken.None);
        r.IsFailure.Should().BeTrue();
        r.Error.Code.Should().Be("Sap.WriteBlockedProduction");
        h.Journal.Should().NotContain(j => j.StartsWith("EXEMPTION_", StringComparison.Ordinal));
        h.State.Exemptions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("P", "Sap.WriteBlockedProductionClient")]
    [InlineData(null, "Sap.WriteBlockedCategoryUnknown")]
    public async Task Mandante_de_producao_ou_ilegivel_bloqueia_o_envio(string? category, string code)
    {
        var h = new ExemptionSapHarness(category: category);
        (await h.Sap.SubmitAsync(h.Connection, Apply(), CancellationToken.None)).Error.Code.Should().Be(code);
        h.State.Exemptions.Should().BeEmpty();
        // a simulação é só leitura: continua liberada
        (await h.Sap.PreviewAsync(h.Connection, Apply(), CancellationToken.None)).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Descoberta_traz_politica_motivos_e_aprovadores()
    {
        var h = new ExemptionSapHarness();
        var d = (await h.Sap.DiscoverAsync(h.Connection, CancellationToken.None)).Value;
        d.Policy.AllowsLocal.Should().BeTrue();
        d.ApplyPublished.Should().BeTrue();
        d.Customizing.Reasons.Select(r => r.Id).Should().Equal("FPOS", "OTHR");
        d.Approvers.Should().Be(1);
        var off = new ExemptionSapHarness(new ExemptionScenario { Policy = null });
        var policy = (await off.Sap.GetPolicyAsync(off.Connection, CancellationToken.None)).Value;
        (policy.AllowsLocal, policy.KeyExists).Should().Be((false, false));
        (await off.Sap.PreviewAsync(off.Connection, Apply(), CancellationToken.None)).Value.Status.Should().Be(ExemptionRequestStatus.Blocked);
    }
}
