using FluentAssertions;
using Mignow.Atc.Infra.External.Sap.Adt;

namespace Mignow.Atc.Tests.Extraction;

/// <summary>Parsing do XML de <c>/sap/bc/adt/atc/results/{id}</c> (formato lido pelo extrair.ps1; XML sintético).</summary>
public sealed class AtcResultXmlTests
{
    private const string Xml = """
        <?xml version="1.0" encoding="utf-8"?>
        <atcobject:objects xmlns:atcobject="http://www.sap.com/adt/atc/object" xmlns:atcfinding="http://www.sap.com/adt/atc/finding"
                           xmlns:adtcore="http://www.sap.com/adt/core" xmlns:atom="http://www.w3.org/2005/Atom">
          <atcobject:object adtcore:uri="/sap/bc/adt/programs/programs/zmig_a" adtcore:type="PROG" adtcore:name="ZMIG_A" adtcore:packageName="ZPKG">
            <atcobject:findings>
              <atcfinding:finding adtcore:uri="/sap/bc/adt/atc/results/X/items/itemid/0a1B2c" atcfinding:priority="1" atcfinding:checkTitle="Chk"
                                  atcfinding:messageTitle="Msg" atcfinding:exemptionApproval="-" atcfinding:quickfixInfo="MK1">
                <atom:link rel="http://www.sap.com/adt/relations/documentation" href="/docu/1"/>
                <atcfinding:quickfixes atcfinding:manual="true"/>
                <atcfinding:tags>
                  <atcfinding:tag atcfinding:name="REF_OBJ_NAME" atcfinding:value="KONV"/>
                  <atcfinding:tag atcfinding:name="NOTE" atcfinding:value="0002385939"/>
                  <atcfinding:tag atcfinding:name="ADD_INFO" atcfinding:value="x"/>
                </atcfinding:tags>
              </atcfinding:finding>
              <atcfinding:finding adtcore:uri="/sap/bc/adt/atc/results/X/items/itemid/FFEE" atcfinding:priority="2" atcfinding:checkTitle="Chk2"/>
            </atcobject:findings>
          </atcobject:object>
          <atcobject:object adtcore:uri="/sap/bc/adt/oo/classes/zcl_b" adtcore:type="CLAS" adtcore:name="ZCL_B">
            <atcobject:findings/>
          </atcobject:object>
        </atcobject:objects>
        """;

    [Fact]
    public void Colunas_de_objeto_finding_e_tags_na_ordem_do_script()
    {
        var r = AdtAtcXml.ParseFindings(Xml);
        r.Rows.Should().HaveCount(2);
        r.ObjectColumns.Should().Equal("obj_uri", "obj_type", "obj_name", "obj_packageName");
        r.FindingColumns.Should().Equal("uri", "priority", "checkTitle", "messageTitle", "exemptionApproval", "quickfixInfo", "quickfix_manual");
        r.TagColumns.Should().Equal("tag_ADD_INFO", "tag_NOTE", "tag_REF_OBJ_NAME");
    }

    [Fact]
    public void Item_id_documentacao_e_valores_do_finding()
    {
        var f = AdtAtcXml.ParseFindings(Xml).Rows[0];
        f["item_id"].Should().Be("0A1B2C");
        f["documentation_link"].Should().Be("/docu/1");
        f["obj_name"].Should().Be("ZMIG_A");
        f["tag_NOTE"].Should().Be("0002385939");
        f["quickfix_manual"].Should().Be("true");
        AdtAtcXml.ParseFindings(Xml).Rows[1]["item_id"].Should().Be("FFEE");
    }
}
