namespace Mignow.Atc.Domain.Remediation;

/// <summary>Entrada de <c>historico_status</c> ({data, status}): base do burndown do relatório final.</summary>
public sealed record StatusChange(DateTimeOffset At, string Status);
