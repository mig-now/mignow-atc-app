using Mignow.Atc.Domain.Extraction;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>Variação por prioridade (P1, P2, P3) dos findings sem baseline.</summary>
public sealed record PriorityVariation(int P1, int P2, int P3);

/// <summary>
/// Variação contra a extração anterior (<c>resultado.py</c>): <see cref="New"/> = Δ dos findings sem baseline ("c/ baseline"
/// da skill, <c>variacao_com_baseline</c>), <see cref="Total"/> = Δ do total, <see cref="NewByPriority"/> por prioridade.
/// </summary>
public sealed record ExtractionVariation(string PreviousDisplayId, DateTimeOffset? PreviousExecutedAtUtc, int Total, int New, PriorityVariation NewByPriority)
{
    public static ExtractionVariation Compute(ExtractionCounts current, AtcExtraction previous) =>
        new(previous.DisplayId, previous.ExecutedAtUtc, current.Total - previous.TotalCount, current.New - (previous.TotalCount - previous.InBaselineCount),
            new PriorityVariation(current.P1New - previous.P1New, current.P2New - previous.P2New, current.P3New - previous.P3New));
}
