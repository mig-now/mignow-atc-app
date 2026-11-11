using System.Xml.Linq;
using FluentAssertions;
using Mignow.Atc.Infra.External.Sap.Adt;

namespace Mignow.Atc.Tests.Sap;

public sealed class AdtXmlTests
{
    [Fact]
    public void Ativacao_de_include_leva_o_contexto_do_programa()
    {
        var body = AdtXml.ActivationBody("/sap/bc/adt/programs/includes/zinc", "zinc", "/sap/bc/adt/programs/programs/zprog");
        var reference = XDocument.Parse(body).Descendants().Single(e => e.Name.LocalName == "objectReference");
        reference.Attributes().Single(a => a.Name.LocalName == "uri").Value
            .Should().Be("/sap/bc/adt/programs/includes/zinc?context=%2Fsap%2Fbc%2Fadt%2Fprograms%2Fprograms%2Fzprog");
        reference.Attributes().Single(a => a.Name.LocalName == "name").Value.Should().Be("ZINC");
    }

    [Fact]
    public void Mensagens_da_ativacao_seguem_a_regex_da_skill()
    {
        const string body = "<chkl:messages><msg objDescr=\"x\" type=\"E\" line=\"3\"><shortText><txt>Campo &quot;X&quot; desconhecido</txt></shortText></msg>" +
                            "<msg type=\"W\"><shortText><txt>aviso</txt></shortText></msg></chkl:messages>";
        AdtXml.ActivationMessages(body).Select(m => (m.Severity, m.Text)).Should().Equal(("E", "Campo \"X\" desconhecido"), ("W", "aviso"));
    }

    [Fact]
    public void Lock_handle_e_request_do_lock()
    {
        const string body = "<asx:abap><asx:values><DATA><LOCK_HANDLE>ABC123</LOCK_HANDLE><CORRNR>DI4K900001</CORRNR></DATA></asx:values></asx:abap>";
        AdtXml.LockHandle(body).Should().Be("ABC123");
        AdtXml.LockCorrNr(body).Should().Be("DI4K900001");
        AdtXml.LockHandle("<x/>").Should().BeNull();
    }

    [Fact]
    public void Fontes_do_objectstructure_sem_repetir_e_com_o_nome_do_include()
    {
        const string xml = """
            <abapsource:objectStructureElement xmlns:abapsource="x" xmlns:atom="y">
              <atom:link href="/sap/bc/adt/programs/programs/zprog/source/main#start=1"/>
              <atom:link href="/sap/bc/adt/programs/includes/zprog_top/source/main"/>
              <atom:link href="/sap/bc/adt/programs/includes/zprog_top/source/main?x"/>
            </abapsource:objectStructureElement>
            """;
        AdtXml.StructureSources(xml).Select(p => (p.Name, p.Uri)).Should().Equal(
            ("ZPROG", "/sap/bc/adt/programs/programs/zprog/source/main"),
            ("ZPROG_TOP", "/sap/bc/adt/programs/includes/zprog_top/source/main"));
    }

    [Fact]
    public void Mensagem_de_erro_http_e_curta()
    {
        AdtXml.Message(403, "<exc:exception><message lang=\"EN\">Objeto bloqueado por USER1</message></exc:exception>")
            .Should().Be("HTTP 403: Objeto bloqueado por USER1");
    }
}
