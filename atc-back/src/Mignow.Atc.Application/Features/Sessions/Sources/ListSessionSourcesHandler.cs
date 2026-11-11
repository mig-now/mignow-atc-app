using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Sources;

public sealed class ListSessionSourcesHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<ListSessionSourcesQuery, Result<SessionSourcesResponse>>
{
    public async Task<Result<SessionSourcesResponse>> Handle(ListSessionSourcesQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var id = session.Id;
        var items = await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == id).Select(i => new { i.ObjectType, i.ObjectName, i.Status }), ct);
        // objetos com código (tipos de dicionário não têm fonte a ler)
        var objects = items.Where(i => i.ObjectName.Length > 0 && !StandardObjects.NoCodeTypes.Contains(i.ObjectType.ToUpperInvariant()))
            .GroupBy(i => (Type: i.ObjectType.ToUpperInvariant(), Name: i.ObjectName.ToUpperInvariant()))
            .ToList();
        var names = objects.Select(g => g.Key.Name).Distinct().ToArray();
        var systemId = session.SapSystemId;
        var caches = await store.ListAsync(store.Read<SourceCache>().Where(c => c.SapSystemId == systemId && names.Contains(c.ObjectName))
            .Select(c => new { c.Guid, c.ObjectName, c.IncludeName, c.Source, c.ReadAt }), ct);
        var byName = caches.GroupBy(c => c.ObjectName).ToDictionary(g => g.Key, g => g.ToList());
        var read = objects.Where(g => byName.ContainsKey(g.Key.Name))
            .OrderBy(g => g.Key.Name, StringComparer.Ordinal).ThenBy(g => g.Key.Type, StringComparer.Ordinal)
            .Select(g => new SessionSourceObject(g.Key.Type, g.Key.Name, g.Count(), g.Count(i => i.Status == RemediationStatus.Pending),
                [.. byName[g.Key.Name].OrderBy(c => c.IncludeName, StringComparer.Ordinal)
                    .Select(c => new SessionSourceInclude(c.Guid, c.IncludeName, LineCount(c.Source), c.ReadAt))]))
            .ToList();
        return new SessionSourcesResponse(objects.Count, read.Count, read);
    }

    private static int LineCount(string source) => source.Length == 0 ? 0 : source.Count(ch => ch == '\n') + (source.EndsWith('\n') ? 0 : 1);
}
