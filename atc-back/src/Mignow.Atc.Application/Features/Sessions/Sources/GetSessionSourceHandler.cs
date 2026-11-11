using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Sources;

/// <summary>Só fonte do ambiente da sessão e de objeto que tem item na sessão.</summary>
public sealed class GetSessionSourceHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetSessionSourceQuery, Result<SessionSourceContent>>
{
    public async Task<Result<SessionSourceContent>> Handle(GetSessionSourceQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var systemId = session.SapSystemId;
        var cache = await store.FirstOrDefaultAsync(store.Read<SourceCache>().Where(c => c.Guid == query.SourceId && c.SapSystemId == systemId), ct);
        if (cache is null)
            return l.NotFound("Source.NotFound");
        var id = session.Id;
        var name = cache.ObjectName;
        var inSession = await store.FirstOrDefaultAsync(store.Read<RemediationItem>().Where(i => i.SessionId == id && i.ObjectName.ToUpper() == name), ct);
        if (inSession is null)
            return l.NotFound("Source.NotFound");
        return new SessionSourceContent(cache.Guid, cache.ObjectName, cache.IncludeName, cache.AdtUri, cache.ReadAt, cache.Source);
    }
}
