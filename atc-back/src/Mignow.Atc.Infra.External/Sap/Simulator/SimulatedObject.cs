using Mignow.Atc.Application.Ports.Sap;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

public sealed class SimulatedObject
{
    public required SapObjectRef Ref { get; init; }

    /// <summary>Versão mais recente (inativa quando <see cref="Inactive"/>).</summary>
    public required string Source { get; set; }

    public required string ActiveSource { get; set; }

    public bool Inactive { get; set; }

    public string? LockedBy { get; set; }

    public string? OpenRequest { get; set; }

    public List<string> Dependencies { get; init; } = [];

    public List<string> UsedBy { get; init; } = [];

    public string? MainProgram { get; init; }
}
