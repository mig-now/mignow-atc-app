namespace Mignow.Atc.Domain.Report;

/// <summary>
/// Premissas da comparação manual × IA trocadas para este cliente (<c>premissas_manual</c> do <c>complementos.json</c>).
/// Campo nulo = vale o padrão de <c>GeneralSettings</c> (ManualObjectsPerDay / HoursPerDay).
/// </summary>
public sealed record ManualPremise(int? ObjectsPerDay, int? HoursPerDay, string? Source);
