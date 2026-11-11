using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Knowledge.GetElement;

public sealed class GetKbElementHandler(IKnowledgeBase kb, IStringLocalizer<Errors> l) : IQueryHandler<GetKbElementQuery, Result<KbElementDetail>>
{
    public async Task<Result<KbElementDetail>> Handle(GetKbElementQuery query, CancellationToken ct)
    {
        var result = await KbFailures.RunAsync(l, () => kb.GetElementAsync(query.Category, query.Element, ct));
        if (result.IsFailure)
            return result.Error;
        return result.Value is null ? l.NotFound("Knowledge.ElementNotFound") : result.Value;
    }
}
