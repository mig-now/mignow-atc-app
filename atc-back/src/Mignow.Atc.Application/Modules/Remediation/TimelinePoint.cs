namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Situação ao fim de um dia com movimentação (burndown por finding e por objeto).</summary>
public sealed record TimelinePoint(DateOnly Date, SituationCounts Situation);
