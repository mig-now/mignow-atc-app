using FluentAssertions;
using Mignow.Atc.Application.Ports.Sap.Exemption;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Infra.External.Sap.Adt;

namespace Mignow.Atc.Tests.Exemption;

/// <summary>XML do pedido e da resposta, fiéis ao <c>AtcIsencao.psm1</c> / <c>solicitar.ps1</c>.</summary>
public sealed class ExemptionXmlTests
{
    private const string Template =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><atcexmpt:exemptionApply xmlns:atcexmpt=\"http://www.sap.com/adt/atc/exemption\" xmlns:atcfinding=\"http://www.sap.com/adt/atc/finding\" xmlns:adtcore=\"http://www.sap.com/adt/core\">" +
        "<atcexmpt:exemptionProposal><atcfinding:finding adtcore:name=\"ZPROG\" adtcore:type=\"{0}\" atcfinding:checksum=\"{1}\"/>" +
        "<atcexmpt:restriction><atcexmpt:thisFinding enabled=\"{2}\">false</atcexmpt:thisFinding><atcexmpt:rangeOfFindings enabled=\"true\"/></atcexmpt:restriction>" +
        "<atcexmpt:approver/><atcexmpt:reason/><atcexmpt:justification/><atcexmpt:notify/></atcexmpt:exemptionProposal></atcexmpt:exemptionApply>";

    private static string T(string type = "PROG/P", string checksum = "123", bool thisFinding = true) =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture, Template, type, checksum, thisFinding ? "true" : "false");

    private static readonly ExemptionApply Apply = new("atc:AAA,1,  X", "approver1", "FPOS", "FPOS - POS - ATC - MIGNOW | S/4 MIGRATION MIG-NOW 08.11.2026 AUT-TS: a & b", "U");

    [Fact]
    public void Pedido_preenche_so_o_que_cabe_ao_consultor_e_restringe_ao_finding()
    {
        var template = ExemptionXml.ParseTemplate(200, T());
        template.HasProposal.Should().BeTrue();
        template.ThisFinding.Should().BeTrue();
        var xml = ExemptionXml.BuildRequest(ExemptionXml.Proposal(template.Xml)!, Apply);

        xml.Should().StartWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?><atcexmpt:exemptionProposal");
        xml.Should().Contain("<atcexmpt:thisFinding enabled=\"true\">true</atcexmpt:thisFinding>");
        xml.Should().Contain("rangeOfFindings enabled=\"false\"");
        xml.Should().Contain("<atcexmpt:approver>APPROVER1</atcexmpt:approver>");
        xml.Should().Contain("<atcexmpt:reason>FPOS</atcexmpt:reason>");
        xml.Should().Contain("a &amp; b");
        xml.Should().Contain("<atcexmpt:notify>on_rejection</atcexmpt:notify>");
        xml.Should().Contain("<atcexmpt:validity>U</atcexmpt:validity>");
        xml.Should().Contain("<atcexmpt:validUntil>9999-12-31</atcexmpt:validUntil>");
        xml.Should().Contain("atcfinding:checksum=\"123\"");
    }

    [Fact]
    public void Validade_diferente_de_U_nao_grava_valid_until()
    {
        var xml = ExemptionXml.BuildRequest(ExemptionXml.Proposal(T())!, Apply with { Validity = "P" });
        xml.Should().Contain("<atcexmpt:validity>P</atcexmpt:validity>").And.NotContain("validUntil");
    }

    [Theory]
    [InlineData("PROG/P", "-1", "finding sem checksum (-1)")]
    [InlineData("AQSG", "55", "objeto ABAP Query (AQSG)")]
    [InlineData("CLAS/OC", "55", "tipo CLAS/OC, checksum 55")]
    public void Modelo_sem_escopo_do_finding_e_SEM_ESCOPO(string type, string checksum, string reason)
    {
        var template = ExemptionXml.ParseTemplate(200, T(type, checksum, thisFinding: false));
        template.ThisFinding.Should().BeFalse();
        template.ScopeReason.Should().Be(reason);
        var attempt = ExemptionXml.Evaluate(template);
        attempt.Status.Should().Be(ExemptionRequestStatus.NoScope);
        attempt.RequestXml.Should().BeNull();
        ((Action)(() => ExemptionXml.BuildRequest(ExemptionXml.Proposal(template.Xml)!, Apply))).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Status_sem_modelo_e_BLOQUEADO_com_a_mensagem_do_SAP()
    {
        const string body = "<atcexmpt:exemptionApply xmlns:atcexmpt=\"http://www.sap.com/adt/atc/exemption\"><atcexmpt:status><atcexmpt:message>Exemptions disabled by system setup</atcexmpt:message><atcexmpt:type>E</atcexmpt:type></atcexmpt:status></atcexmpt:exemptionApply>";
        var attempt = ExemptionXml.Evaluate(ExemptionXml.ParseTemplate(200, body));
        attempt.Status.Should().Be(ExemptionRequestStatus.Blocked);
        attempt.Message.Should().Be("Exemptions disabled by system setup");
    }

    [Fact]
    public void Http_diferente_de_200_no_modelo_vira_mensagem_HTTP()
    {
        var t = ExemptionXml.ParseTemplate(406, "x");
        (t.HasProposal, t.Message, t.Type).Should().Be((false, "HTTP 406", "E"));
    }

    [Theory]
    [InlineData(200, "S", "PEDIDO")]
    [InlineData(200, "I", "INFO")]
    [InlineData(200, "E", "ERRO")]
    [InlineData(400, "S", "ERRO")]
    public void Status_do_envio(int http, string type, string expected) => ExemptionXml.SubmitStatus(http, type).Should().Be(expected);

    [Fact]
    public void Resposta_le_mensagem_e_tipo_e_excecao_adt()
    {
        ExemptionXml.ParseStatus("<atcexmpt:status><atcexmpt:message>Exemption request &amp; ok</atcexmpt:message><atcexmpt:type>S</atcexmpt:type></atcexmpt:status>")
            .Should().Be(("Exemption request & ok", "S"));
        ExemptionXml.ParseStatus("<exc:exception><localizedMessage lang=\"EN\">Invalid data</localizedMessage><properties><entry key=\"XML_PATH\">/validUntil</entry></properties></exc:exception>")
            .Should().Be(("Invalid data (/validUntil)", "E"));
    }

    [Fact]
    public void Customizing_totais_e_colecoes()
    {
        var c = ExemptionXml.ParseCustomizing("<reasons><reason id=\"FPOS\" justificationMandatory=\"false\" title=\"False Positive\"/><reason id=\"OTHR\" justificationMandatory=\"true\" title=\"Other\"/></reasons><validity id=\"U\" value=\"Unlimited\"/>");
        c.Reasons.Should().Equal(new ExemptionReason("FPOS", false, "False Positive"), new ExemptionReason("OTHR", true, "Other"));
        c.Validities.Should().Equal("U = Unlimited");
        ExemptionXml.ParseTotals("<atcexempt:countTotal>3</atcexempt:countTotal><atcexempt:numOpen>1</atcexempt:numOpen>")
            .Should().BeEquivalentTo(new Dictionary<string, int> { ["countTotal"] = 3, ["numOpen"] = 1 });
        ExemptionXml.Collections("<a href=\"/sap/bc/adt/atc/exemptions/apply\"/><b href=\"/sap/bc/adt/atc/exemptions/apply\"/><c href=\"/sap/bc/adt/atc/checkexemptionsview{?x}\"/>")
            .Should().Equal("/sap/bc/adt/atc/checkexemptionsview{?x}", "/sap/bc/adt/atc/exemptions/apply");
    }

    [Theory]
    [InlineData("A", true)]
    [InlineData("C", false)]
    [InlineData("X", false)]
    [InlineData("", false)]
    public void Politica_so_aceita_pedido_local_com_A(string value, bool allows) => ExemptionPolicy.From(value, true).AllowsLocal.Should().Be(allows);

    [Fact]
    public void Politica_texto_como_o_resultado_py() =>
        ExemptionPolicy.From(null, false).Display.Should().Be("(vazio) – nenhum resultado (isencoes desativadas)");
}
