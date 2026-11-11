namespace Mignow.Atc.Application.Dtos.Scans;

/// <summary>Regras do <c>regras.json</c> (versão, gatilho e cada regra com o refinamento).</summary>
public sealed record ScanRulesResponse(string? Version, string? Date, string Trigger, IReadOnlyList<ScanRuleResponse> Rules);
