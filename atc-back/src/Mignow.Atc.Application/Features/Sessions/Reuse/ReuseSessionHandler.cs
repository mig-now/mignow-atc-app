using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Reuse;

public sealed class ReuseSessionHandler(IDataStore store, IDateTimeProvider clock, IStringLocalizer<Errors> l) : IQueryHandler<ReuseSessionQuery, Result<ReuseResult>>
{
    public async Task<Result<ReuseResult>> Handle(ReuseSessionQuery command, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, command.SessionId, false, ct);
        var origin = await SessionViews.LoadAsync(store, command.FromSessionId, false, ct);
        if (session is null || origin is null)
            return l.NotFound("Session.NotFound");
        if (session.SapSystemId != origin.SapSystemId || session.Id == origin.Id)
            return l.Validation("Session.ReuseOtherSystem");
        var (sid, oid) = (session.Id, origin.Id);
        var targets = await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == sid).OrderBy(i => i.N), ct);
        var originItems = await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == oid && i.CorrectionSheetMd != null), ct);
        var sources = await SessionViews.SourcesAsync(store, session.SapSystemId, [.. targets.Select(i => i.ObjectName)], ct);
        return SheetReuse.Reuse(targets, originItems, command.ItemNs, sources, session.Author, DateOnly.FromDateTime(clock.UtcNow.ToLocalTime().DateTime));
    }
}
