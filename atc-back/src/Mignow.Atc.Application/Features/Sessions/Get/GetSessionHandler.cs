using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Sessions.Get;

public sealed class GetSessionHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetSessionQuery, Result<SessionResponse>>
{
    public async Task<Result<SessionResponse>> Handle(GetSessionQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.Id, false, ct);
        return session is null ? l.NotFound("Session.NotFound") : (await SessionViews.BuildAsync(store, [session], ct))[0];
    }
}
