using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Reports.Highlights;

public sealed record GetHighlightsQuery(Guid CustomerId) : IQuery<Result<HighlightsResponse>>;
