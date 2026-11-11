namespace Mignow.Atc.Application.Dtos.Scans;

/// <summary><see cref="Status"/>: pendente | ja_remediado | informativo; <see cref="Cross"/> só nas pendentes.</summary>
public sealed record ScanOccurrenceResponse(
    Guid Id, string RuleId, string ObjectType, string ObjectName, string? Include, string? AdtSource, int Line, int? LineEnd, string Element,
    string? Snippet, string Status, string? Cross, int Batch);
