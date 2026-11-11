namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Execução do ATC do cenário (o módulo de extração interpreta).</summary>
public sealed record ScenarioAtcRun(string Id, string Title, DateTimeOffset CreatedAt);
