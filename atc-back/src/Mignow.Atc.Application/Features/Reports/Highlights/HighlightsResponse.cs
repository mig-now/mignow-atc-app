using Mignow.Atc.Domain.Report;

namespace Mignow.Atc.Application.Features.Reports.Highlights;

/// <summary>Complementos manuais do relatório do cliente (equivale ao <c>complementos.json</c>).</summary>
public sealed record HighlightsResponse(IReadOnlyList<HighlightText> Highlights, IReadOnlyList<PendingText> Pending, ManualPremise? Premise);
