namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Request de transporte do cenário (<c>Type</c> = Workbench | Customizing).</summary>
public sealed record ScenarioRequest(string Number, string Description, string Type, bool Modifiable, string Owner);
