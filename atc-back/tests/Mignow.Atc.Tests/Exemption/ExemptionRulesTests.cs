using FluentAssertions;
using Mignow.Atc.Application.Features.Exemptions;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Tests.Exemption;

public sealed class ExemptionRulesTests
{
    private static readonly DateTimeOffset Day = new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Justificativa_padrao_tem_prefixo_do_lote_mig_now_data_autor_e_obs_sem_triagem()
    {
        var batch = ExemptionBatch.Create(1, 1, "1", "APPROVER1", null);
        ExemptionRules.Justification(batch, "fpos", "Triagem R5: BAPI ja corrigida", "AUT-MBPL", Day)
            .Should().Be("FPOS - POS - ATC - MIGNOW | S/4 MIGRATION MIG-NOW 08.11.2026 AUT-MBPL: BAPI ja corrigida");
    }

    [Fact]
    public void Prefixo_vazio_e_sem_prefixo_e_o_proprio_texto_vale_com_MOTIVO()
    {
        var none = ExemptionBatch.Create(1, 1, "1", null, string.Empty);
        ExemptionRules.Justification(none, "FPOS", "x", "AUT-TS", Day).Should().Be("S/4 MIGRATION MIG-NOW 08.11.2026 AUT-TS: x");
        var custom = ExemptionBatch.Create(1, 1, "1", null, "{MOTIVO} | ");
        ExemptionRules.Justification(custom, "othr", "x", "AUT-TS", Day).Should().StartWith("OTHR | S/4");
    }

    [Fact]
    public void Aprovador_nao_pode_ser_o_solicitante()
    {
        ExemptionRules.IsRequester("migatc", "MIGATC").Should().BeTrue();
        ExemptionRules.IsRequester("APPROVER1", "MIGATC").Should().BeFalse();
    }

    [Fact]
    public void Prioridades_validas_sao_1_a_3()
    {
        ExemptionRules.ParsePriorities([2, 1, 1]).Should().Equal("1", "2");
        ExemptionRules.ParsePriorities([4]).Should().BeNull();
        ExemptionRules.ParsePriorities([]).Should().BeNull();
    }

    [Fact]
    public void Lote_novo_comeca_criado_e_so_avanca()
    {
        var b = ExemptionBatch.Create(1, 1, "1,2", null, null);
        b.Status.Should().Be(ExemptionBatchStatus.Created);
        b.Advance(ExemptionBatchStatus.Simulated);
        b.Advance(ExemptionBatchStatus.Discovered);
        b.Status.Should().Be(ExemptionBatchStatus.Simulated);
        b.AddRequest(1, "m1", 1, false);
        ((Action)(() => b.AddRequest(2, "m1", 2, false))).Should().Throw<Mignow.Atc.Domain.Common.DomainException>();
    }
}
