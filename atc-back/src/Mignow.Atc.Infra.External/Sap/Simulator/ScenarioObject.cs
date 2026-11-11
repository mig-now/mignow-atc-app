namespace Mignow.Atc.Infra.External.Sap.Simulator;

public sealed record ScenarioObject(string Name, string Type, string Package, string Source, IReadOnlyList<string>? Dependencies = null,
    IReadOnlyList<string>? UsedBy = null, string? MainProgram = null, string? LockedBy = null, string? OpenRequest = null, bool Inactive = false);
