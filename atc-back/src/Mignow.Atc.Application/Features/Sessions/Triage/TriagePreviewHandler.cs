using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Sessions.Triage;

public sealed class TriagePreviewHandler(IDataStore store, IProductRules products, IStringLocalizer<Errors> l) : IQueryHandler<TriagePreviewQuery, Result<TriageResponse>>
{
    public async Task<Result<TriageResponse>> Handle(TriagePreviewQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var (items, result) = await TriageSupport.ClassifyAsync(store, products, session, false, ct);
        return TriageSupport.ToResponse(result, items.Count(i => i.Status.Length == 0), null);
    }
}
