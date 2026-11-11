using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Domain.Scan;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Tests.Integration;

/// <summary>Colunas jsonb dos records do domínio: gravam, voltam iguais e a alteração é detectada.</summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class PersistenceTests(AtcApiFactory api)
{
    [Fact]
    public async Task Item_da_remediacao_guarda_historico_proposta_e_flags_em_jsonb()
    {
        int itemId;
        await using (var scope = api.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AtcDbContext>();
            var system = await db.Set<SapSystem>().FirstAsync(s => s.Host == "sim://migracao-tipica");
            var session = RemediationSession.Create(system.CustomerId, system.Id, RemediationSource.CsvImport, "atc.csv", "1", "SBXK900120", "AUT-TS",
                ProjectType.Conversion, "[\"Priority\",\"Object name\"]", "utf-8-sig", ";", 1);
            db.Add(session);
            await db.SaveChangesAsync();
            var item = RemediationItem.Create(session.Id, 1, "ZPROG", "PROG", "KONV", "TABL", null, null, null, null, "ZP", null, "1", "FID-1", ItemOrigin.Atc,
                "[\"1\",\"ZPROG\"]");
            item.MarkStatus(RemediationStatus.ConfirmedKb, "kb", DateTimeOffset.UtcNow);
            item.SetTriageFlags(["R2"]);
            item.SetProposal(new ItemProposal(ItemProposal.OriginKb, "KONV-001",
                [new ProposalBlock(["SELECT * FROM konv."], ["SELECT * FROM prcd_elements."], 3, "modificacao", null)], null, null, null, [], true, null,
                null, null, null, ItemProposal.ActionFix, null, DateTimeOffset.UtcNow));
            db.Add(item);
            await db.SaveChangesAsync();
            itemId = item.Id;
        }
        await using (var scope = api.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AtcDbContext>();
            var item = await db.Set<RemediationItem>().FirstAsync(i => i.Id == itemId);
            item.StatusHistory.Should().ContainSingle().Which.Status.Should().Be("Confirmado KB");
            item.Proposal!.Blocks.Single().After.Should().Equal("SELECT * FROM prcd_elements.");
            item.TriageFlags.Should().Equal("R2");
            item.MarkStatus(RemediationStatus.Cancelled, "x", DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }
        await using (var scope = api.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AtcDbContext>();
            (await db.Set<RemediationItem>().AsNoTracking().FirstAsync(i => i.Id == itemId)).StatusHistory.Select(h => h.Status)
                .Should().Equal("Confirmado KB", "Cancelado");
        }
    }

    [Fact]
    public async Task Varredura_guarda_os_lotes_em_jsonb()
    {
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AtcDbContext>();
        var system = await db.Set<SapSystem>().FirstAsync(s => s.Host == "sim://migracao-tipica");
        var scan = MignowScan.Create(system.Id, null, "[]");
        scan.SetBatches([new ScanBatch(1, 900, [new ScanObject("PROG", "ZA", "ZP", "A"), new ScanObject("PROG", "ZB", "ZP", "A")], "ZCL_MIGNOW_ATC_SCAN_1", null, false)]);
        db.Add(scan);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await db.Set<MignowScan>().FirstAsync(s => s.Id == scan.Id)).Batches.Single().Objects.Select(o => o.Name).Should().Equal("ZA", "ZB");
    }
}
