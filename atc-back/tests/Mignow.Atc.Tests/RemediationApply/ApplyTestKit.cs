using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Infra.External.Ai.Fake;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Tests.RemediationApply;

/// <summary>IA de teste: a mesma resposta determinística do desenvolvimento (<see cref="RemediationFakeResponder"/>), sem o cadastro de provedores.</summary>
public sealed class ResponderGateway(RemediationFakeResponder responder) : IAiGateway
{
    public Task<Result<T>> CompleteAsync<T>(AiRequest request, Action<AiUsageInfo> onCall, CancellationToken ct)
    {
        var json = responder.Respond(request);
        onCall(new AiUsageInfo("fake-" + request.Usage, 10, 10, 0m));
        return Task.FromResult<Result<T>>(JsonSerializer.Deserialize<T>(json, Json.Options)!);
    }
}

/// <summary>Montagem comum dos testes do módulo: simulador SAP, relógio fixo, KB e IA de mentira.</summary>
public sealed class ApplyTestKit
{
    public const string Source = "REPORT zprog.\nDATA lv_k TYPE i.\nSELECT * FROM konv INTO TABLE lt_konv WHERE knumv = lv_k.\nWRITE 'x'.\n";
    public const string Ddl = "@EndUserText.label : 'Z tab'\ndefine table ztab {\n  key client : abap.clnt not null;\n  key id : abap.char(10) not null;\n  amount : abap.dec(13,2);\n}\n";
    public static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    public ApplyTestKit(Func<SapScenario, SapScenario>? tweak = null)
    {
        var scenario = new SapScenario
        {
            Objects = [new ScenarioObject("ZPROG", "PROG/P", "ZPACK", Source)],
            SyntaxRules = [new ScenarioRule("BAD_STATEMENT", "Comando inválido")],
            DdicObjects = [new ScenarioDdicObject("ZTAB", "TABL", Ddl)],
        };
        Host = $"sim://t{Guid.NewGuid():N}"[..14];
        var env = Substitute.For<IHostEnvironment>();
        var registry = new SimulatedSapRegistry(Options.Create(new SapSimulatorOptions()), env);
        State = registry.Register(Host["sim://".Length..], tweak is null ? scenario : tweak(scenario));
        var l = Substitute.For<IStringLocalizer<Errors>>();
        l[Arg.Any<string>()].Returns(c => new LocalizedString(c.Arg<string>(), c.Arg<string>()));
        l[Arg.Any<string>(), Arg.Any<object[]>()].Returns(c => new LocalizedString(c.Arg<string>(), c.Arg<string>()));
        Sap = new SimulatedSap(registry, l);
        Clock = Substitute.For<IDateTimeProvider>();
        Clock.UtcNow.Returns(Now);
        Localizer = l;
        Kb = Substitute.For<IKnowledgeBase>();
        Ai = new ResponderGateway(new RemediationFakeResponder());
    }

    public string Host { get; }

    public SimulatedSapState State { get; }

    public SimulatedSap Sap { get; }

    public IDateTimeProvider Clock { get; }

    public IStringLocalizer<Errors> Localizer { get; }

    public IKnowledgeBase Kb { get; }

    public IAiGateway Ai { get; }

    public List<(AiUsageInfo Usage, Mignow.Atc.Domain.Registry.AiUsage Purpose, string? ItemRef)> Usages { get; } = [];

    public SapConnection Connection(bool production = false) => new(Guid.NewGuid(), Host, "100", "MIGATC", string.Empty, "EN", null, production);

    public EngineHost HostFor(bool production = false) => EngineHost.Standalone(Connection(production), GeneralSettings.CreateDefault(), Usages);

    public ProposeEngine Propose() => new(Ai, Kb, Sap, Sap, Sap, Clock, Localizer);

    public ApplyEngine Apply() => new(Sap, Sap, Sap, Clock, Localizer);

    public ReviewEngine Review() => new(Ai, Localizer);

    public SourceCache Cache(string source = Source) =>
        SourceCache.Create(1, "ZPROG", "ZPROG", "/sap/bc/adt/programs/programs/zprog/source/main", source, Now);

    public static RemediationItem Item(int n, string objectName = "ZPROG", string objectType = "PROG", string? reference = "KONV", string? refType = "TABL") =>
        RemediationItem.Create(1, n, objectName, objectType, reference, refType, "Check", "Message", "2220005", null, "ZPACK", "Technical", "1", null, ItemOrigin.Atc, "[]");

    public void SetKbCard(string id, string complexity, params string[] alerts) =>
        Kb.LookupAsync("KONV", "TABL", 3, Arg.Any<CancellationToken>()).Returns(new KbLookupResult(
            [new KbCard(id, 5, "padrão", "SELECT * FROM konv", "SELECT * FROM prcd_elements", "elementos/tabelas/KONV.md", false, alerts, complexity, null)], 1));

    public ProposeInput Input(string mode, IReadOnlyList<RemediationItem> items, IReadOnlyList<SourceCache> caches) =>
        new(mode, "AUT-ML", DateOnly.FromDateTime(Now.LocalDateTime), items, caches, []);
}
