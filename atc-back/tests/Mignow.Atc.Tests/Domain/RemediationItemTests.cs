using FluentAssertions;
using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.Domain;

/// <summary>MarkStatus = porte de <c>marcar()</c> do atc.py: histórico só quando o status muda; status/obs/data sempre.</summary>
public sealed class RemediationItemTests
{
    private static RemediationItem NewItem() =>
        RemediationItem.Create(1, 7, "ZPROG", "PROG", "KONV", "TABL", "Check", "Msg", "2220005", null, "ZPKG", "Technical", "1", null, ItemOrigin.Atc, "[]");

    [Fact]
    public void Muda_status_e_acrescenta_historico()
    {
        var item = NewItem();
        var t1 = new DateTimeOffset(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);
        item.MarkStatus(RemediationStatus.ConfirmedKb, "cartão X", t1);
        item.Status.Should().Be("Confirmado KB");
        item.Obs.Should().Be("cartão X");
        item.StatusDate.Should().Be(t1);
        item.StatusHistory.Should().Equal(new StatusChange(t1, "Confirmado KB"));
    }

    [Fact]
    public void Mesmo_status_nao_duplica_o_historico_mas_atualiza_obs_e_data()
    {
        var item = NewItem();
        var t1 = DateTimeOffset.UtcNow;
        var t2 = t1.AddMinutes(5);
        item.MarkStatus(RemediationStatus.Exemption, "R5", t1);
        item.MarkStatus(RemediationStatus.Exemption, "R5 revisto", t2);
        item.StatusHistory.Should().HaveCount(1);
        item.Obs.Should().Be("R5 revisto");
        item.StatusDate.Should().Be(t2);
    }

    [Fact]
    public void Voltar_para_pendente_tambem_entra_no_historico()
    {
        var item = NewItem();
        item.MarkStatus(RemediationStatus.Exemption, "R1", DateTimeOffset.UtcNow);
        item.MarkStatus(RemediationStatus.Pending, "Conferência R1: chave", DateTimeOffset.UtcNow);
        item.StatusHistory.Select(h => h.Status).Should().Equal("Isenção", "");
    }

    [Fact]
    public void Primeira_marcacao_como_pendente_nao_gera_historico()
    {
        var item = NewItem();
        item.MarkStatus(RemediationStatus.Pending, "x", DateTimeOffset.UtcNow);
        item.StatusHistory.Should().BeEmpty("em marcar() o status inicial já é vazio");
    }

    [Theory]
    [InlineData("Confirmado")]
    [InlineData("confirmado kb")]
    [InlineData("Isencao")]
    public void Status_fora_dos_textos_da_skill_e_recusado(string status) =>
        ((Action)(() => NewItem().MarkStatus(status, null, DateTimeOffset.UtcNow))).Should().Throw<DomainException>()
            .Which.Code.Should().Be("Item.InvalidStatus");

    [Fact]
    public void Textos_exatos_da_skill() =>
        RemediationStatus.All.Should().Equal("", "Confirmado KB", "Confirmado IA", "Cancelado", "Isenção");
}
