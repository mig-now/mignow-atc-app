using FluentAssertions;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Tests.RemediationApply;

/// <summary>Alertas da KB (porte de <c>alertas()</c> do atc.py).</summary>
public sealed class AlertsTests
{
    [Fact]
    public void Material_em_18_posicoes() =>
        ProposalAlerts.Of("lv_matnr(18) = ls-matnr.").Should().ContainSingle().Which.Should().Contain("MATERIAL_LONG");

    [Fact]
    public void Alpha_so_alerta_quando_ha_material()
    {
        ProposalAlerts.Of("* ALPHA\nCALL FUNCTION 'CONVERSION_EXIT_ALPHA_INPUT' EXPORTING input = lv_matnr.").Should().ContainSingle().Which.Should().Contain("MATN1");
        ProposalAlerts.Of("* ALPHA\nCALL FUNCTION 'CONVERSION_EXIT_ALPHA_INPUT' EXPORTING input = lv_kunnr.").Should().BeEmpty();
    }

    [Fact]
    public void Troca_para_transacao_enjoy_mantendo_bdc()
    {
        ProposalAlerts.Of("CALL TRANSACTION 'ME52N' USING bdcdata.", "CALL TRANSACTION 'ME52' USING bdcdata.").Should().ContainSingle().Which.Should().Contain("ME52N");
        ProposalAlerts.Of("CALL TRANSACTION 'ME52N' USING bdcdata.", "CALL TRANSACTION 'ME51' USING bdcdata.").Should().BeEmpty();
    }

    [Fact]
    public void Select_into_table_dentro_de_loop()
    {
        ProposalAlerts.Of("LOOP AT lt.\n SELECT * FROM t INTO TABLE lt2.\nENDLOOP.").Should().Contain(ProposalAlerts.LoopAlert);
        ProposalAlerts.Of("LOOP AT lt.\n SELECT * FROM t APPENDING TABLE lt2.\nENDLOOP.").Should().NotContain(ProposalAlerts.LoopAlert);
    }
}
