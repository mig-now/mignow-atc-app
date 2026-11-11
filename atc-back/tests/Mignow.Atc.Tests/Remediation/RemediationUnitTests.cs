using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.Database.Context;
using NSubstitute;

namespace Mignow.Atc.Tests.Remediation;

public sealed class RemediationUnitTests
{
    [Fact]
    public void Csv_le_cp1252_virgula_aspas_e_linha_em_branco_e_escreve_igual()
    {
        var text = "a,b,c\r\n1,\"x, \"\"y\"\"\",ç\r\n\r\n";
        var raw = CodePagesEncodingProvider.Instance.GetEncoding(1252)!.GetBytes(text);
        var doc = AtcCsv.Read(raw)!;
        (doc.Encoding, doc.Delimiter).Should().Be(("cp1252", ","));
        doc.Rows.Should().HaveCount(2);
        doc.Rows[0].Should().Equal("1", "x, \"y\"", "ç");
        doc.Rows[1].Should().BeEmpty("linha em branco = lista vazia, como o csv.reader");
        AtcCsv.Write(doc.Encoding, doc.Delimiter, doc.Header, doc.Rows).Should().Equal(raw);
    }

    [Fact]
    public void Importar_exige_colunas_e_prioridade()
    {
        SessionImporter.Import(new CsvDocument("utf-8-sig", ";", ["A", "B"], []), null).ErrorCode.Should().Be("Session.CsvColumnsMissing");
        SessionImporter.Import(new CsvDocument("utf-8-sig", ";", ["Object name", "Obj."], []), "1").ErrorCode.Should().Be("Session.CsvPriorityColumnMissing");
        SessionImporter.ParseScope(" 2, 1,2 ").Should().Equal("1", "2");
        SessionImporter.ParseScope("todas").Should().BeNull();
    }

    [Fact]
    public void Linha_em_branco_ou_sem_tipo_nao_vira_item_mas_mantem_a_numeracao()
    {
        var doc = new CsvDocument("utf-8-sig", ";", ["Object name", "Obj.", "Priority"], [["ZA", "PROG", "1"], [], ["ZB", "PROG", "1"]]);
        var o = SessionImporter.Import(doc, null);
        o.Rows.Select(r => r.N).Should().Equal(1, 3);
        o.Summary!.IgnoredRows.Should().Be(1);
    }

    [Fact]
    public async Task Reimportar_arquiva_o_ciclo_anterior_e_preserva_status_quando_n_e_objeto_coincidem()
    {
        var db = new AtcDbContext(new DbContextOptionsBuilder<AtcDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var store = new EfDataStore(db);
        var customer = Customer.Create("C", "C");
        db.Add(customer);
        await db.SaveChangesAsync();
        var system = SapSystem.Create(customer, "DI4", "400", "sim://x", 0, false, "EN", null, false, ProjectType.Upgrade, "AUT-TS", "REQ1", null, "U");
        db.Add(system);
        await db.SaveChangesAsync();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(DateTimeOffset.UtcNow);
        var creator = new SessionCreator(store, clock, Substitute.For<IStringLocalizer<Errors>>());
        var header = new[] { "Object name", "Obj.", "Referenced Object", "RObT", "Priority" };
        CsvDocument Doc(params string[][] rows) => new("utf-8-sig", ";", header, rows);

        var first = (await creator.CreateSessionFromRowsAsync(system, new SessionSource(RemediationSource.CsvImport, "a.csv", null, null, Doc(
            ["ZA", "PROG", "KONV", "TABL", "1"], ["ZB", "PROG", "BSEG", "TABL", "1"])), CancellationToken.None)).Value;
        await db.SaveChangesAsync();
        first.Session.Cycle.Should().Be(1);
        first.Session.Request.Should().Be("REQ1");
        var item = await db.Set<RemediationItem>().SingleAsync(i => i.N == 1);
        item.MarkStatus(RemediationStatus.ConfirmedKb, "ok", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        var second = (await creator.CreateSessionFromRowsAsync(system, new SessionSource(RemediationSource.CsvImport, "b.csv", "1", null, Doc(
            ["ZA", "PROG", "KONV", "TABL", "1"], ["OUTRO", "PROG", "BSEG", "TABL", "1"], ["ZC", "PROG", "KONV", "TABL", "2"])), CancellationToken.None)).Value;
        await db.SaveChangesAsync();

        second.Session.Cycle.Should().Be(2);
        second.ArchivedSessions.Should().Be(1);
        second.CarriedOver.Should().Be(1, "só o n=1 mantém o mesmo objeto");
        (await db.Set<RemediationSession>().CountAsync(s => s.Archived)).Should().Be(1);
        var items = await db.Set<RemediationItem>().Where(i => i.SessionId == second.Session.Id).OrderBy(i => i.N).ToListAsync();
        items.Select(i => i.Status).Should().Equal("Confirmado KB", string.Empty);
        items[0].StatusHistory.Should().HaveCount(1);
        second.Session.ExcludedRowsJson.Should().Contain("ZC");
    }

    [Fact]
    public async Task Origem_do_item_e_mignow_na_varredura_ou_com_a_coluna_Origem_MIGNOW()
    {
        var db = new AtcDbContext(new DbContextOptionsBuilder<AtcDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var store = new EfDataStore(db);
        var customer = Customer.Create("C", "C");
        db.Add(customer);
        await db.SaveChangesAsync();
        var system = SapSystem.Create(customer, "DI4", "400", "sim://x", 0, false, "EN", null, false, ProjectType.Upgrade, "AUT-TS", "REQ1", null, "U");
        db.Add(system);
        await db.SaveChangesAsync();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(DateTimeOffset.UtcNow);
        var creator = new SessionCreator(store, clock, Substitute.For<IStringLocalizer<Errors>>());
        string[] header = ["Object name", "Obj.", "Referenced Object", "RObT", "Priority", "Origem"];
        var doc = new CsvDocument("utf-8-sig", ";", header, [["ZA", "PROG", "KONV", "TABL", "1", "MIGNOW"], ["ZB", "PROG", "BSEG", "TABL", "1", ""]]);

        var csv = (await creator.CreateSessionFromRowsAsync(system, new SessionSource(RemediationSource.CsvImport, "a.csv", null, null, doc), CancellationToken.None)).Value;
        await db.SaveChangesAsync();
        (await db.Set<RemediationItem>().Where(i => i.SessionId == csv.Session.Id).OrderBy(i => i.N).Select(i => i.Origin).ToListAsync())
            .Should().Equal(ItemOrigin.Mignow, ItemOrigin.Atc);

        var scan = (await creator.CreateSessionFromRowsAsync(system, new SessionSource(RemediationSource.MignowScan, "v1", null, null, doc), CancellationToken.None)).Value;
        await db.SaveChangesAsync();
        (await db.Set<RemediationItem>().Where(i => i.SessionId == scan.Session.Id).Select(i => i.Origin).Distinct().ToListAsync())
            .Should().Equal(ItemOrigin.Mignow);
    }

    [Fact]
    public void Historico_antes_depois_transicoes_e_tempo_liquido_sem_pausas_longas()
    {
        var t0 = new DateTimeOffset(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);
        var items = new List<HistoryItem>
        {
            new(1, "PROG ZA", "Confirmado KB", [new StatusChange(t0.AddMinutes(5), "Confirmado KB")]),
            new(2, "PROG ZA", "", []),
            new(3, "PROG ZB", "Isenção", [new StatusChange(t0.AddHours(3), "Isenção")]),
        };
        var h = SessionHistoryBuilder.Build(t0, items, []);
        h.Before.Pending.Should().Be(3);
        (h.After.Kb, h.After.Exemption, h.After.Pending, h.After.PendingObjects).Should().Be((1, 1, 1, 1));
        h.Transitions.Should().BeEquivalentTo(new Dictionary<string, int> { ["Pendente → KB"] = 1, ["Pendente → Isenção"] = 1 });
        h.NetSeconds.Should().Be(0, "duas marcações a mais de 10 min uma da outra não somam tempo");
        h.WallSeconds.Should().Be((int)TimeSpan.FromMinutes(175).TotalSeconds, "do 1º ao último registro");
        SessionHistoryBuilder.Category("Confirmado IA").Should().Be("IA");
    }

    [Fact]
    public void Ficha_preserva_a_aplicacao_no_sap_e_o_reaproveitamento_monta_o_bloco()
    {
        var item = RemediationItem.Create(1, 3, "ZA", "PROG", "KONV", "TABL", "Chk", "Msg", "2220005", null, "ZP", null, "1", null, ItemOrigin.Atc, "[]");
        var old = "x\n**Aplicação no SAP:** ATIVADO · request R1\n";
        var sheet = CorrectionSheet.Render(item, "Confirmado KB", "obs", "SELECT * FROM konv.", "*>>> S/4 MIGRATION MIG-NOW - START OF INSERTION - 01.01.2026 AUT-A\nSELECT * FROM prcd.\n*<<< S/4 MIGRATION MIG-NOW - END OF INSERTION - 01.01.2026 AUT-A", old);
        sheet.Should().Contain("**Aplicação no SAP:** ATIVADO · request R1").And.Contain("# Item 3 – PROG ZA");
        item.MarkStatus(RemediationStatus.ConfirmedKb, "obs", DateTimeOffset.UtcNow);
        item.SetCorrectionSheet(sheet);
        var target = RemediationItem.Create(1, 9, "ZA", "PROG", "KONV", "TABL", null, null, null, null, "ZP", null, "1", null, ItemOrigin.Atc, "[]");
        var sources = new SourceIndex([new SourceFileText("ZA", "ZA", "REPORT za.\nSELECT * FROM konv.\n")]);
        var r = SheetReuse.Reuse([target], [item], null, sources, "AUT-TS", new DateOnly(2026, 11, 7));
        var c = r.Items.Single();
        c.Situation.Should().Be("ok");
        c.Block!.First().Should().Be("*>>> S/4 MIGRATION MIG-NOW - START OF DELETION - 07.11.2026 AUT-TS");
        c.Occurrences!.Single().Line.Should().Be(2);
    }
}
