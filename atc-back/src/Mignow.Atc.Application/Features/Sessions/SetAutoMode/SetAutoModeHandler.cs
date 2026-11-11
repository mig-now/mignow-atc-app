using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Sessions.SetAutoMode;

public sealed class SetAutoModeHandler(IDataStore store, IStringLocalizer<Errors> l) : ICommandHandler<SetAutoModeCommand, Result<SessionResponse>>
{
    public async Task<Result<SessionResponse>> Handle(SetAutoModeCommand c, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, c.SessionId, true, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        session.SetAutoMode(c.Enabled);
        return (await SessionViews.BuildAsync(store, [session], ct))[0];
    }
}
