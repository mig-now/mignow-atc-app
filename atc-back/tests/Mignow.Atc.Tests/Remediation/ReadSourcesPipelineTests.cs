using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Remediation;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.Database.Context;
using Mignow.Atc.Infra.External.Sap.Simulator;
using NSubstitute;

namespace Mignow.Atc.Tests.Remediation;

/// <summary>Run ReadSources (plano + leitura) e ConfirmR1 no simulador, com o EF em memória: lote, cache, standard no fim e DDIC.</summary>
public sealed class ReadSourcesPipelineTests
{
    private sealed record Env(AtcDbContext Db, EfDataStore Store, SimulatedSap Sap, SapSystem System, Customer Customer, RemediationSession Session, SimulatedSapState State);

    private static async Task<Env> SetupAsync(Func<SapScenario, SapScenario>? tweak = null)
    {
        var db = new AtcDbContext(new DbContextOptionsBuilder<AtcDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var store = new EfDataStore(db);
        var registry = new SimulatedSapRegistry(Options.Create(new SapSimulatorOptions()), Substitute.For<IHostEnvironment>());
        var scenario = new SapScenario
        {
            Objects =
            [
                new ScenarioObject("ZPROG", "PROG/P", "ZPKG", "REPORT zprog.\nINCLUDE zprog_i.\nSELECT * FROM konv INTO TABLE lt.\n", ["ZPROG_I"]),
                new ScenarioObject("ZPROG_I", "PROG/I", "ZPKG", "WRITE 'x'.\n", null, null, "ZPROG"),
                new ScenarioObject("SAPMV45A", "PROG/P", "SAP", "REPORT sapmv45a.\n"),
            ],
            Tables = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>
            {
                ["DD02L"] = [new Dictionary<string, string> { ["TABNAME"] = "MARA", ["AS4LOCAL"] = "A" }],
                ["DD01L"] = [new Dictionary<string, string> { ["DOMNAME"] = "ZDOM_A", ["ENTITYTAB"] = "MARA" }],
            },
        };
        var name = $"t{Guid.NewGuid():N}"[..12];
        var state = registry.Register(name, tweak is null ? scenario : tweak(scenario));
        var customer = Customer.Create("Cliente", "CLI");
        db.Add(customer);
        await db.SaveChangesAsync();
        var system = SapSystem.Create(customer, "SIM", "100", $"sim://{name}", 0, false, "EN", null, false, ProjectType.Conversion, "AUT-TS", "SIMK900001", null, "DEV");
        db.Add(system);
        await db.SaveChangesAsync();
        var session = RemediationSession.Create(customer.Id, system.Id, RemediationSource.CsvImport, "x.csv", "1", null, "AUT-TS", ProjectType.Conversion, "[]", "utf-8-sig", ";", 1);
        session.AddItems(
        [
            RemediationItem.Create(0, 1, "ZPROG", "PROG", "KONV", "TABL", null, null, null, null, "ZPKG", null, "1", null, ItemOrigin.Atc, "[]"),
            RemediationItem.Create(0, 2, "ZTAB_A", "TABL", "MARA", "TABL", null, null, "2220005", null, "ZPKG", "Ambiguous", "1", null, ItemOrigin.Atc, "[]"),
            RemediationItem.Create(0, 3, "ZDOM_A", "DOMA", "MARA", "TABL", null, null, "2220005", null, "ZPKG", "Ambiguous", "1", null, ItemOrigin.Atc, "[]"),
            RemediationItem.Create(0, 4, "SAPMV45A", "PROG", "KONV", "TABL", null, null, null, null, "SAP", null, "1", null, ItemOrigin.Atc, "[]"),
        ]);
        db.Add(session);
        await db.SaveChangesAsync();
        return new Env(db, store, new SimulatedSap(registry, Localizer()), system, customer, session, state);
    }

    private static IStringLocalizer<Errors> Localizer()
    {
        var l = Substitute.For<IStringLocalizer<Errors>>();
        l[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        l[Arg.Any<string>(), Arg.Any<object[]>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        return l;
    }

    private static RunContext Context(Env e, RunKind kind, object parameters, string[] steps)
    {
        var run = Run.Create(kind, e.Customer.Id, e.System.Id, e.Session.Guid, Json.Write(parameters), "test", steps, DateTimeOffset.UtcNow);
        e.Db.Add(run);
        e.Db.SaveChanges();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(_ => DateTimeOffset.UtcNow);
        var conn = new SapConnection(e.System.Guid, e.System.BaseUrl, "100", "DEV", string.Empty, "EN", null, false);
        return new RunContext(run, e.System, e.Customer, conn, GeneralSettings.CreateDefault(), e.Store, e.Store, Substitute.For<IRunNotifier>(), clock, TimeSpan.Zero, CancellationToken.None);
    }

    [Fact]
    public async Task Le_o_lote_com_includes_pula_sem_codigo_e_deixa_standard_sem_confirmacao()
    {
        var e = await SetupAsync();
        var ctx = Context(e, RunKind.ReadSources, new ReadSourcesParameters(e.Session.Guid, 40, false, false),
            [RemediationStepCodes.ReadSourcesPlan, RemediationStepCodes.ReadSourcesFetch]);

        (await new ReadSourcesPlanStep().ExecuteAsync(ctx)).StopRun.Should().BeFalse();
        (await new ReadSourcesFetchStep(e.Sap).ExecuteAsync(ctx)).StopRun.Should().BeFalse();

        var cache = await e.Db.Set<SourceCache>().ToListAsync();
        cache.Select(c => (c.ObjectName, c.IncludeName)).Should().BeEquivalentTo([("ZPROG", "ZPROG"), ("ZPROG", "ZPROG_I")]);
        cache.Single(c => c.IncludeName == "ZPROG").Source.Should().Contain("konv");
        ctx.Run.ResultJson.Should().Contain("\"read\":1").And.Contain("standardNotRead");
        e.State.Journal.Count(c => c.StartsWith("SOURCES ", StringComparison.Ordinal)).Should().Be(1);

        // repetir = cache (idempotente, sem novas leituras no SAP)
        (await new ReadSourcesFetchStep(e.Sap).ExecuteAsync(ctx)).StopRun.Should().BeFalse();
        ctx.Run.ResultJson.Should().Contain("\"cached\":1");
        e.State.Journal.Count(c => c.StartsWith("SOURCES ", StringComparison.Ordinal)).Should().Be(1);
    }

    [Fact]
    public async Task Com_include_standard_le_tambem_o_standard()
    {
        var e = await SetupAsync();
        var ctx = Context(e, RunKind.ReadSources, new ReadSourcesParameters(e.Session.Guid, 40, true, false),
            [RemediationStepCodes.ReadSourcesPlan, RemediationStepCodes.ReadSourcesFetch]);
        await new ReadSourcesPlanStep().ExecuteAsync(ctx);
        await new ReadSourcesFetchStep(e.Sap).ExecuteAsync(ctx);
        (await e.Db.Set<SourceCache>().Select(c => c.ObjectName).Distinct().ToListAsync()).Should().BeEquivalentTo(["ZPROG", "SAPMV45A"]);
    }

    [Fact]
    public async Task ConfirmR1_grava_isencao_so_chave_e_nao_altera_item_com_erro_de_consulta()
    {
        var e = await SetupAsync();
        foreach (var n in new[] { 2, 3 })
            (await e.Db.Set<RemediationItem>().SingleAsync(i => i.N == n)).MarkStatus(RemediationStatus.Cancelled, "Triagem R1: objeto sem código ABAP, finding Ambiguous (confirmar na SE11)", DateTimeOffset.UtcNow);
        await e.Db.SaveChangesAsync();
        var ctx = Context(e, RunKind.ConfirmR1, new ConfirmR1Parameters(e.Session.Guid, null, true), [RemediationStepCodes.ConfirmR1Check, RemediationStepCodes.ConfirmR1Apply]);
        var sql = Substitute.For<ISapSql>();
        sql.SelectAsync(Arg.Any<SapConnection>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci => e.Sap.SelectAsync(ci.ArgAt<SapConnection>(0), ci.ArgAt<string>(1), ci.ArgAt<int>(2), ci.ArgAt<CancellationToken>(3)));

        await new ConfirmR1CheckStep(sql).ExecuteAsync(ctx);
        await new ConfirmR1ApplyStep().ExecuteAsync(ctx);

        var items = await e.Db.Set<RemediationItem>().OrderBy(i => i.N).ToListAsync();
        items[2].Status.Should().Be("Isenção");
        items[2].Obs.Should().StartWith("Conferência R1: DOMA ZDOM_A referencia MARA só como chave/verificação");
        items[1].Status.Should().Be("Cancelado", "consulta que o SAP recusou (erro) não altera o item");
        ctx.Run.ResultJson.Should().Contain("\"error\":[2]");
    }
}
