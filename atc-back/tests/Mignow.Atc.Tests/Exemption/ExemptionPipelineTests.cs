using FluentAssertions;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Exemption;
using Mignow.Atc.Application.Features.Exemptions.Create;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.External.Sap.Simulator;
using NSubstitute;

namespace Mignow.Atc.Tests.Exemption;

/// <summary>
/// Pipeline completo no simulador, sem banco: lote (listar.py) → descobrir → simular → enviar → conferir, com os passos
/// reais, o RunContext real e um IDataStore em memória.
/// </summary>
public sealed class ExemptionPipelineTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 15, 0, 0, TimeSpan.Zero);

    private sealed class World
    {
        public required FakeDataStore Store { get; init; }
        public required ExemptionSapHarness Sap { get; init; }
        public required SapSystem System { get; init; }
        public required ExemptionBatch Batch { get; init; }
        public required IDateTimeProvider Clock { get; init; }
    }

    private static void SetId(object entity, int id) => typeof(EntityBase).GetProperty(nameof(EntityBase.Id))!.SetValue(entity, id);

    private static async Task<World> CreateAsync(ExemptionSapHarness sap, string? approver = "APPROVER1")
    {
        var store = new FakeDataStore();
        var customer = store.Seed(Customer.Create("Cliente", "cli"));
        var system = SapSystem.Create(customer, "SBX", "100", sap.Connection.BaseUrl, 0, false, "EN", null, false, ProjectType.Conversion, "AUT-TS", null, approver, "MIGATC");
        store.Seed(system);
        var session = store.Seed(RemediationSession.Create(customer.Id, system.Id, RemediationSource.Extraction, null, "1,2", null, "AUT-TS", ProjectType.Conversion,
            "[]", "utf-8-sig", ";", 1));
        var markers = new[] { ExemptionSapHarness.Normal, ExemptionSapHarness.NoScope, ExemptionSapHarness.Syntax, ExemptionSapHarness.Blocked, "atc:N2,5,  X" };
        for (var n = 1; n <= markers.Length; n++)
        {
            var item = RemediationItem.Create(session.Id, n, $"ZOBJ{n}", "PROG", "BAPI_X", "BAPI", "CHECK", "Call of BAPI", null, null, null, null, "1", markers[n - 1],
                ItemOrigin.Atc, "[]");
            item.MarkStatus(RemediationStatus.Exemption, "Triagem R5: BAPI ja corrigida", Now);
            store.Seed(item);
        }
        var l = Substitute.For<IStringLocalizer<Errors>>();
        l[Arg.Any<string>(), Arg.Any<object[]>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        l[Arg.Any<string>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        var created = await new CreateExemptionBatchHandler(store, l).Handle(new CreateExemptionBatchCommand(session.Guid, [1]), CancellationToken.None);
        created.IsSuccess.Should().BeTrue(created.IsSuccess ? string.Empty : created.Error.Code);
        created.Value.Report.Findings.Should().Be(5);
        created.Value.Report.ProbablyNoScope.Should().BeEmpty();   // sem extração em memória, o aviso vem só da simulação
        var batch = store.Table<ExemptionBatch>().Cast<ExemptionBatch>().Single();
        batch.SetParameters(approver, "FPOS", "U", null);
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(Now);
        return new World { Store = store, Sap = sap, System = system, Batch = batch, Clock = clock };
    }

    private static async Task<(StepOutcome Outcome, RunContext Ctx)> RunAsync(World w, RunKind kind, IRunStep step, IReadOnlyList<int>? seq = null, SapSystem? system = null)
    {
        var run = Run.Create(kind, 1, w.System.Id, w.Batch.Guid, Json.Write(new ExemptionRunParameters(w.Batch.Guid, seq)), "tester", [step.Code], Now);
        w.Store.Seed(run);
        var uow = Substitute.For<IUnitOfWork>();
        var ctx = new RunContext(run, w.System, w.System.Customer, w.Sap.Connection, null!, w.Store, uow, Substitute.For<IRunNotifier>(), w.Clock, TimeSpan.Zero,
            CancellationToken.None) { CurrentStep = step.Code };
        return (await step.ExecuteAsync(ctx), ctx);
    }

    private static Dictionary<string, string?> Statuses(World w) =>
        w.Batch.Requests.ToDictionary(r => r.MarkerId, r => r.Status);

    [Fact]
    public async Task Fluxo_completo_descobrir_simular_enviar_conferir()
    {
        var sap = new ExemptionSapHarness();
        var w = await CreateAsync(sap);
        w.Batch.Status.Should().Be(ExemptionBatchStatus.Created);

        var discover = await RunAsync(w, RunKind.ExemptionDiscover, new ExemptionDiscoverStep(sap.Sap));
        discover.Outcome.StopRun.Should().BeFalse();
        w.Batch.Status.Should().Be(ExemptionBatchStatus.Discovered);
        w.Batch.DiscoveryJson.Should().Contain("\"allowsLocal\":true");

        var simulate = await RunAsync(w, RunKind.ExemptionSimulate, new ExemptionSimulateStep(sap.Sap));
        simulate.Outcome.StopRun.Should().BeFalse();
        w.Batch.Status.Should().Be(ExemptionBatchStatus.Simulated);
        Statuses(w).Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            [ExemptionSapHarness.Normal] = "SIMULADO", ["atc:N2,5,  X"] = "SIMULADO", [ExemptionSapHarness.NoScope] = "SEM_ESCOPO",
            [ExemptionSapHarness.Syntax] = "SEM_ESCOPO", [ExemptionSapHarness.Blocked] = "BLOQUEADO",
        });
        var first = w.Batch.Requests.Single(r => r.MarkerId == ExemptionSapHarness.Normal);
        first.Justification.Should().Be("FPOS - POS - ATC - MIGNOW | S/4 MIGRATION MIG-NOW " + Now.ToLocalTime().ToString("dd.MM.yyyy") + " AUT-TS: BAPI ja corrigida");
        first.RequestXml.Should().Contain("<atcexmpt:approver>APPROVER1</atcexmpt:approver>");
        sap.State.Exemptions.Should().BeEmpty("simular não grava no SAP");

        var send = await RunAsync(w, RunKind.ExemptionSend, new ExemptionSendStep(sap.Sap, sap.Sap));
        send.Outcome.StopRun.Should().BeFalse();
        w.Batch.Status.Should().Be(ExemptionBatchStatus.Sent);
        Statuses(w).Where(s => s.Value == "PEDIDO").Select(s => s.Key).Should().BeEquivalentTo([ExemptionSapHarness.Normal, "atc:N2,5,  X"]);
        Statuses(w)[ExemptionSapHarness.NoScope].Should().Be("SEM_ESCOPO", "o que não tem escopo nunca é enviado");
        sap.State.Exemptions.Keys.Should().BeEquivalentTo([ExemptionSapHarness.Normal, "atc:N2,5,  X"]);
        first.ResponseXml.Should().Contain("Exemption request created");

        var check = await RunAsync(w, RunKind.ExemptionCheck, new ExemptionCheckStep(sap.Sap));
        check.Outcome.StopRun.Should().BeFalse();
        w.Batch.Status.Should().Be(ExemptionBatchStatus.Checked);
        check.Ctx.Run.ResultJson.Should().Contain("Exemption already requested").And.Contain("\"requested\":2");
        check.Ctx.Run.ResultJson.Should().Contain("\"findings\":2");

        // simular de novo não desfaz o que já foi pedido
        await RunAsync(w, RunKind.ExemptionSimulate, new ExemptionSimulateStep(sap.Sap));
        Statuses(w)[ExemptionSapHarness.Normal].Should().Be("PEDIDO");
    }

    [Fact]
    public async Task Motivos_diferentes_por_seq_e_enviar_so_alguns()
    {
        var sap = new ExemptionSapHarness();
        var w = await CreateAsync(sap);
        await RunAsync(w, RunKind.ExemptionDiscover, new ExemptionDiscoverStep(sap.Sap));
        await RunAsync(w, RunKind.ExemptionSimulate, new ExemptionSimulateStep(sap.Sap), [1]);
        w.Batch.SetParameters("APPROVER1", "OTHR", "U", null);
        await RunAsync(w, RunKind.ExemptionSimulate, new ExemptionSimulateStep(sap.Sap), [5]);
        w.Batch.Requests.Single(r => r.Seq == 1).Reason.Should().Be("FPOS");
        var other = w.Batch.Requests.Single(r => r.Seq == 5);
        other.Reason.Should().Be("OTHR");
        other.Justification.Should().StartWith("OTHR - POS - ATC - MIGNOW | ");
        w.Batch.Requests.Where(r => r.Seq is 2 or 3 or 4).Should().OnlyContain(r => r.Status == null);

        await RunAsync(w, RunKind.ExemptionSend, new ExemptionSendStep(sap.Sap, sap.Sap), [1]);
        Statuses(w)[ExemptionSapHarness.Normal].Should().Be("PEDIDO");
        Statuses(w)["atc:N2,5,  X"].Should().Be("SIMULADO");
        sap.State.Exemptions.Should().ContainSingle().Which.Value.Reason.Should().Be("FPOS");
    }

    [Fact]
    public async Task Politica_diferente_de_A_para_a_descoberta_e_o_envio()
    {
        var sap = new ExemptionSapHarness(new ExemptionScenario { Policy = "C" });
        var w = await CreateAsync(sap);
        var discover = await RunAsync(w, RunKind.ExemptionDiscover, new ExemptionDiscoverStep(sap.Sap));
        discover.Outcome.StopRun.Should().BeTrue();
        discover.Outcome.MessageCode.Should().Be("Exemption.Discovery.PolicyNotAllowed");
        w.Batch.Status.Should().Be(ExemptionBatchStatus.Created);
        w.Batch.DiscoveryJson.Should().Contain("\"ok\":false");

        var send = await RunAsync(w, RunKind.ExemptionSend, new ExemptionSendStep(sap.Sap, sap.Sap));
        send.Outcome.MessageCode.Should().Be("Exemption.Send.PolicyDisabled");
        sap.State.Exemptions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, "T", "Sap.WriteBlockedProduction")]
    [InlineData(false, "P", "Sap.WriteBlockedProductionClient")]
    [InlineData(false, null, "Sap.WriteBlockedCategoryUnknown")]
    public async Task Envio_em_producao_e_bloqueado_antes_de_qualquer_pedido(bool production, string? category, string code)
    {
        var sap = new ExemptionSapHarness(category: category, production: production);
        var w = await CreateAsync(sap);
        await RunAsync(w, RunKind.ExemptionDiscover, new ExemptionDiscoverStep(sap.Sap));
        await RunAsync(w, RunKind.ExemptionSimulate, new ExemptionSimulateStep(sap.Sap));
        var send = await RunAsync(w, RunKind.ExemptionSend, new ExemptionSendStep(sap.Sap, sap.Sap));
        send.Outcome.StopRun.Should().BeTrue();
        send.Outcome.MessageCode.Should().Be(code);
        sap.State.Exemptions.Should().BeEmpty();
        sap.Journal.Should().NotContain(j => j.StartsWith("EXEMPTION_POST", StringComparison.Ordinal));
        Statuses(w).Values.Should().NotContain("PEDIDO");
    }

    [Fact]
    public async Task Aprovador_igual_ao_solicitante_para_o_envio()
    {
        var sap = new ExemptionSapHarness();
        var w = await CreateAsync(sap, approver: "MIGATC");
        await RunAsync(w, RunKind.ExemptionSimulate, new ExemptionSimulateStep(sap.Sap));
        var send = await RunAsync(w, RunKind.ExemptionSend, new ExemptionSendStep(sap.Sap, sap.Sap));
        send.Outcome.MessageCode.Should().Be("Exemption.ApproverIsRequester");
        sap.State.Exemptions.Should().BeEmpty();
    }

    [Fact]
    public async Task Motivo_inexistente_no_customizing_para_a_simulacao()
    {
        var sap = new ExemptionSapHarness();
        var w = await CreateAsync(sap);
        w.Batch.SetParameters("APPROVER1", "XXXX", "U", null);
        var simulate = await RunAsync(w, RunKind.ExemptionSimulate, new ExemptionSimulateStep(sap.Sap));
        simulate.Outcome.MessageCode.Should().Be("Exemption.ReasonUnknown");
    }
}
