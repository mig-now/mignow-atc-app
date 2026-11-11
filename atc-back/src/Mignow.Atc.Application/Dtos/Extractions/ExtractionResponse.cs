using System.Text.Json;
using Mignow.Atc.Domain.Extraction;

namespace Mignow.Atc.Application.Dtos.Extractions;

/// <summary>
/// Extração: contagens (total, em baseline, novos = sem baseline), P1/P2/P3 no total e nos novos, variação contra a extração
/// anterior e dados da execução do ATC (JSON livre). <see cref="FileBaseName"/> = nome-base dos CSVs.
/// </summary>
public sealed record ExtractionResponse(
    Guid Id, Guid SapSystemId, string Series, string DisplayId, string? Title, DateTimeOffset? ExecutedAtUtc, DateTimeOffset ExtractedAt, int TotalCount,
    int InBaselineCount, int NewCount, PriorityCounts Total, PriorityCounts New, JsonElement? Variation, JsonElement? Execution,
    ExtractionSituation Situation, string FileBaseName);
