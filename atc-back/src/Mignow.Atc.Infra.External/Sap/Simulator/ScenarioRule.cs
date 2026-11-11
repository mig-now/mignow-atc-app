namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Padrão no fonte (sem diferenciar maiúsculas) que gera erro de sintaxe ou achado do ATC na linha onde aparece.</summary>
public sealed record ScenarioRule(string Pattern, string Message, string? Code = null, string? Check = null, int Priority = 1);
