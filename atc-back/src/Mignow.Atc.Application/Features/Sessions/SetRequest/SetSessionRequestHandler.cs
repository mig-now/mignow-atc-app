using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Sessions.SetRequest;

public sealed class SetSessionRequestHandler(IDataStore store, IStringLocalizer<Errors> l) : ICommandHandler<SetSessionRequestCommand, Result<SessionResponse>>
{
    public async Task<Result<SessionResponse>> Handle(SetSessionRequestCommand c, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, c.SessionId, true, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        if (session.Archived)
            return l.Conflict("Session.Archived");
        session.SetRequest(c.Request.Trim());
        return (await SessionViews.BuildAsync(store, [session], ct))[0];
    }
}
