using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.GetItem;

public sealed class GetSessionItemHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetSessionItemQuery, Result<ItemDetailResponse>>
{
    public async Task<Result<ItemDetailResponse>> Handle(GetSessionItemQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var sessionId = session.Id;
        var item = await store.FirstOrDefaultAsync(store.Read<RemediationItem>().Where(i => i.SessionId == sessionId && i.Guid == query.ItemId), ct);
        return item is null ? l.NotFound("Session.ItemNotFound") : item.ToDetail();
    }
}
