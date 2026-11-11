using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Infra.External.Sap.Simulator;
using NSubstitute;

namespace Mignow.Atc.Tests.Exemption;

/// <summary>Simulador com cenário em memória (política de isenção por teste) e o localizador que devolve a própria chave.</summary>
internal sealed class ExemptionSapHarness
{
    public const string Normal = "atc:N1,1,  X";
    public const string NoScope = "atc:Q1,2,  X";
    public const string Syntax = "atc:S1,3,  X";
    public const string Blocked = "atc:B1,4,  X";

    public ExemptionSapHarness(ExemptionScenario? exemption = null, string? category = "T", bool production = false)
    {
        var env = Substitute.For<IHostEnvironment>();
        env.ContentRootPath.Returns(AppContext.BaseDirectory);
        var registry = new SimulatedSapRegistry(Options.Create(new SapSimulatorOptions()), env);
        var name = $"x{Guid.NewGuid():N}"[..10];
        State = registry.Register(name, new SapScenario
        {
            ClientCategory = category,
            Exemption = exemption ?? new ExemptionScenario
            {
                Findings =
                [
                    new ScenarioExemptionFinding { MarkerId = NoScope, ObjectName = "ZQRY", Type = "AQSG" },
                    new ScenarioExemptionFinding { MarkerId = Syntax, ObjectName = "ZSYN", Checksum = "-1" },
                    new ScenarioExemptionFinding { MarkerId = Blocked, BlockMessage = "Exemption already requested" },
                ],
            },
        });
        var l = Substitute.For<IStringLocalizer<Errors>>();
        l[Arg.Any<string>(), Arg.Any<object[]>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        l[Arg.Any<string>()].Returns(c => new LocalizedString((string)c[0], (string)c[0]));
        Sap = new SimulatedSap(registry, l);
        Connection = new SapConnection(Guid.NewGuid(), $"sim://{name}", "100", "MIGATC", string.Empty, "EN", null, production);
    }

    public SimulatedSapState State { get; }

    public SimulatedSap Sap { get; }

    public SapConnection Connection { get; }

    public IReadOnlyList<string> Journal => State.Journal;
}
