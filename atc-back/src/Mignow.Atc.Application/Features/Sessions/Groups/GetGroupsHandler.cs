using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.Settings;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Groups;

public sealed class GetGroupsHandler(IDataStore store, IKnowledgeBase kb, IStringLocalizer<Errors> l) : IQueryHandler<GetGroupsQuery, Result<GroupingResult>>
{
    public async Task<Result<GroupingResult>> Handle(GetGroupsQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var id = session.Id;
        var items = await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == id).OrderBy(i => i.N), ct);
        var batch = query.Batch is > 0 ? query.Batch.Value : (await SettingsStore.LoadAsync(store, false, ct)).BatchSize;
        var lote = GroupEngine.BatchItems(items, batch);
        var sources = await SessionViews.SourcesAsync(store, session.SapSystemId, [.. lote.Select(i => i.ObjectName)], ct);
        return await GroupEngine.GroupAsync(items, batch, sources, kb, ct);
    }
}
