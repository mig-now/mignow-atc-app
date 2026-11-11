using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.History;

public sealed class GetSessionHistoryHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetSessionHistoryQuery, Result<SessionHistory>>
{
    public async Task<Result<SessionHistory>> Handle(GetSessionHistoryQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var id = session.Id;
        var items = await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == id).OrderBy(i => i.N), ct);
        var guid = session.Guid;
        var runs = await store.ListAsync(store.Read<Run>().Where(r => r.SubjectId == guid).OrderBy(r => r.QueuedAt), ct);
        return SessionHistoryBuilder.Build(session.CreatedAt,
            [.. items.Select(i => new HistoryItem(i.N, $"{i.ObjectType} {i.ObjectName}", i.Status, i.StatusHistory))],
            [.. runs.Select(r => new HistoryRun(r.Guid, r.Kind, r.Status, r.QueuedAt, r.StartedAt, r.FinishedAt, r.Parallelism, r.AiCalls, r.AiInputTokens, r.AiOutputTokens, r.AiCost))]);
    }
}
