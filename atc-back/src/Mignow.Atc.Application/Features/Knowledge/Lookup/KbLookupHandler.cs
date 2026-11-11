using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Knowledge.Lookup;

public sealed class KbLookupHandler(IKnowledgeBase kb, IStringLocalizer<Errors> l) : IQueryHandler<KbLookupQuery, Result<KbLookupResult>>
{
    public Task<Result<KbLookupResult>> Handle(KbLookupQuery query, CancellationToken ct) =>
        KbFailures.RunAsync(l, () => kb.LookupAsync(query.Ref, query.Robt?.Trim().ToUpperInvariant(), query.Top, ct));
}
