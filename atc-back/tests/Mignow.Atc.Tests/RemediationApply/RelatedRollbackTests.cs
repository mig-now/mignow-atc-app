using FluentAssertions;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Tests.RemediationApply;

/// <summary>Classe + interface relacionada: o backup do relacionado é guardado na aplicação e o rollback devolve os dois fontes (<c>-Rollback -Objeto</c> do script).</summary>
public sealed class RelatedRollbackTests
{
    private const string ClassSource = "CLASS zcl_a DEFINITION PUBLIC.\n  PUBLIC SECTION.\n    INTERFACES zif_a.\nENDCLASS.\nCLASS zcl_a IMPLEMENTATION.\n  METHOD zif_a~go.\n    SELECT * FROM konv INTO TABLE lt.\n  ENDMETHOD.\nENDCLASS.\n";
    private const string IntfSource = "INTERFACE zif_a PUBLIC.\n  METHODS go IMPORTING iv TYPE matnr.\nENDINTERFACE.\n";
    private const string ClassUri = "/sap/bc/adt/oo/classes/zcl_a/source/main";
    private const string IntfUri = "/sap/bc/adt/oo/interfaces/zif_a/source/main";

    [Fact]
    public async Task Aplicacao_guarda_o_backup_do_relacionado_e_o_rollback_restaura_principal_e_relacionado()
    {
        var kit = new ApplyTestKit(s => s with
        {
            Objects = [new ScenarioObject("ZCL_A", "CLAS/OC", "ZPACK", ClassSource), new ScenarioObject("ZIF_A", "INTF/OI", "ZPACK", IntfSource)],
        });
        var classCache = SourceCache.Create(1, "ZCL_A", "ZCL_A", ClassUri, ClassSource, ApplyTestKit.Now);
        var intfCache = SourceCache.Create(1, "ZIF_A", "ZIF_A", IntfUri, IntfSource, ApplyTestKit.Now);
        var item = ApplyTestKit.Item(1, "ZCL_A", "CLAS");
        var blocks = new List<ProposalBlock>
        {
            new(["    SELECT * FROM konv INTO TABLE lt."], ["    SELECT * FROM prcd_elements INTO TABLE lt."], 7, "modificacao", null),
            new(["  METHODS go IMPORTING iv TYPE matnr."], ["  METHODS go IMPORTING iv TYPE matnr_long."], 2, "modificacao", "ZIF_A"),
        };
        item.SetProposal(new ItemProposal(ItemProposal.OriginAi, null, blocks, null, "x", "x", [], null, null, null, null, "teste", ItemProposal.ActionFix, null, ApplyTestKit.Now));
        item.SetPreparation(new ItemPreparation("ZCL_A", ClassUri, [], "x", "x", [new RelatedPreparation("ZIF_A", "INTF", "ZIF_A", IntfUri, "x", "x")], ApplyTestKit.Now));
        item.MarkStatus(RemediationStatus.ConfirmedAi, "ok", ApplyTestKit.Now);
        var input = new ApplyInput("SBXK900001", "AUT-ML", "MIGATC", [item], [classCache, intfCache], false, new Dictionary<int, DdicCreation>());

        var applied = await kit.Apply().ApplyAsync(input, kit.HostFor());
        applied.Reports.Single().Result.Should().Be("ATIVADO");
        item.Application!.BackupSource.Should().Be(ClassSource, "o backup do principal não é sobrescrito pelo do relacionado");
        item.Application.RelatedBackups.Should().ContainSingle().Which.Should().Be(new RelatedBackup("ZIF_A", "INTF", IntfUri, IntfSource));
        kit.State.WithLock(() => kit.State.Get("ZIF_A")!.ActiveSource).Should().Contain("matnr_long");

        var rolled = await kit.Apply().RollbackAsync(input, kit.HostFor());
        rolled.Reports.Single().Result.Should().Be("ROLLBACK_ATIVADO");
        kit.State.WithLock(() => kit.State.Get("ZCL_A")!.ActiveSource).Should().Be(ClassSource);
        kit.State.WithLock(() => kit.State.Get("ZIF_A")!.ActiveSource).Should().Be(IntfSource);
        item.Application.RelatedBackups.Should().ContainSingle("o backup do relacionado permanece");
    }
}
