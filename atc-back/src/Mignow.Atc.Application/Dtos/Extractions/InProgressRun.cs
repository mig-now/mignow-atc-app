namespace Mignow.Atc.Application.Dtos.Extractions;

/// <summary>Execução do ATC mais nova que a última concluída, ainda em andamento (será a próxima extração).</summary>
public sealed record InProgressRun(string DisplayId, string Series, string? Title, DateTimeOffset? ScheduledAtUtc);
