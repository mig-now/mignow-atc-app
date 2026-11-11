namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Resumo da importação (campos do <c>out</c> do <c>cmd_importar</c>).</summary>
public sealed record ImportSummary(
    int Items,
    IReadOnlyDictionary<string, int> ByType,
    int DistinctObjects,
    string Priorities,
    IReadOnlyDictionary<string, int> ByPriorityInCsv,
    int OutOfScope,
    int ShiftedColumns,
    int IgnoredRows);
