using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Knowledge.Index;

public sealed class GetKbIndexHandler(IKnowledgeBase kb, IStringLocalizer<Errors> l) : IQueryHandler<GetKbIndexQuery, Result<KbIndexResponse>>
{
    public Task<Result<KbIndexResponse>> Handle(GetKbIndexQuery query, CancellationToken ct) => KbFailures.RunAsync(l, async () =>
    {
        var elements = await kb.BrowseAsync(ct);
        if (!string.IsNullOrWhiteSpace(query.Category))
            elements = [.. elements.Where(e => e.Category == query.Category)];
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            elements = [.. elements.Where(e => e.Element.Contains(s, StringComparison.OrdinalIgnoreCase) || e.Slug.Contains(s, StringComparison.OrdinalIgnoreCase))];
        }
        return new KbIndexResponse(elements, await kb.BrowsePatternsAsync(ct));
    });
}
