namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Regra da varredura MIGNOW (do <c>regras.json</c>) como a análise a usa: categoria, severidade, descrição e regex refinada.</summary>
public sealed record MignowRuleInfo(string Id, string? Category, string? Severity, string? Description, string? RefinedPcre, string? FixedElement);
