namespace Mignow.Atc.Infra.External.Sap.Simulator;

public sealed class SimulatedDdicObject
{
    public required string Name { get; init; }

    public required string Kind { get; init; }

    public string Package { get; set; } = "ZPACK";

    /// <summary>Versão mais recente (inativa quando <see cref="Inactive"/>).</summary>
    public required string Source { get; set; }

    public required string ActiveSource { get; set; }

    public bool Inactive { get; set; }

    public string? OpenRequest { get; set; }

    public string? LockedBy { get; set; }
}
