using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Domain.Report;

namespace Mignow.Atc.Application.Features.Reports.Highlights;

/// <summary>Substitui os complementos do relatório do cliente (destaques, pendências e premissas da comparação manual × IA).</summary>
public sealed record UpdateHighlightsCommand(Guid CustomerId, IReadOnlyList<HighlightText> Highlights, IReadOnlyList<PendingText> Pending, ManualPremise? Premise)
    : ICommand<Result<HighlightsResponse>>;
