namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Consumo de IA por uso e modelo dentro de uma entrada do histórico.</summary>
public sealed record ProcessModelUsage(string Usage, string Model, int Calls, int FailedCalls, long InputTokens, long OutputTokens, decimal Cost,
    int CallsWithoutCost);
