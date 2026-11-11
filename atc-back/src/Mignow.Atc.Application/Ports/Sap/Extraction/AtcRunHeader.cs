namespace Mignow.Atc.Application.Ports.Sap.Extraction;

/// <summary>
/// Linha de <c>SATC_AC_RESULTH</c>. <see cref="ScheduledOnTs"/> = <c>SCHEDULED_ON_TS</c> como o SAP devolve
/// (<c>AAAAMMDDHHMMSS[.fffffff]</c>, UTC).
/// </summary>
public sealed record AtcRunHeader(
    string DisplayId, string ExecId, string? Title, string Series, string ScheduledOnTs, string? ScheduledBy, bool IsComplete, bool IsReworked,
    string? CheckProfile, string? Configuration, int Prio1, int Prio2, int Prio3, int Prio4, int PlanningErrors);
