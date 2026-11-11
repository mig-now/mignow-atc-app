namespace Mignow.Atc.Application.Dtos.Extractions;

/// <summary>
/// Última execução concluída do ATC de série <c>ZMIG_</c>/<c>Z_MIG_</c> (<c>extrair.ps1 -SoVerificar</c>): série, display id, data
/// (UTC), variante, agendada por, totais por prioridade do cabeçalho, execuções mais novas em andamento e se já foi extraída
/// (<see cref="ExtractionId"/> = a extração existente). <see cref="FileBaseName"/> = nome-base dos CSVs.
/// </summary>
public sealed record AtcLatestResponse(
    Guid SapSystemId, string DisplayId, string Series, string? Title, DateTimeOffset? ExecutedAtUtc, string? ScheduledBy, string? CheckProfile,
    string? Configuration, int Prio1, int Prio2, int Prio3, int Prio4, int PlanningErrors, IReadOnlyList<InProgressRun> InProgress,
    bool AlreadyExtracted, Guid? ExtractionId, string FileBaseName);
