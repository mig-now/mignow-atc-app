using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Monitor;

/// <summary>Sessões (até 200) com contagens por situação e a última execução de cada uma (maior id com <c>SubjectId</c> = sessão).</summary>
public sealed class GetRemediationMonitorHandler(IDataStore store) : IQueryHandler<GetRemediationMonitorQuery, Result<IReadOnlyList<RemediationMonitorRow>>>
{
    private const int Limit = 200;

    public async Task<Result<IReadOnlyList<RemediationMonitorRow>>> Handle(GetRemediationMonitorQuery query, CancellationToken ct)
    {
        var q = store.Read<RemediationSession>();
        if (query.CustomerId is { } cid)
            q = q.Where(s => store.Read<Customer>().Any(x => x.Guid == cid && x.Id == s.CustomerId));
        if (!query.IncludeArchived)
            q = q.Where(s => !s.Archived);
        var sessions = await store.ListAsync(q.OrderByDescending(s => s.CreatedAt).Take(Limit), ct);
        if (sessions.Count == 0)
            return Array.Empty<RemediationMonitorRow>();
        var views = await SessionViews.BuildAsync(store, sessions, ct);

        var guids = sessions.Select(s => (Guid?)s.Guid).ToArray();
        var lastIds = await store.ListAsync(store.Read<Run>().Where(r => guids.Contains(r.SubjectId))
            .GroupBy(r => r.SubjectId).Select(g => g.Max(r => r.Id)), ct);
        var runs = await store.ListAsync(store.Including(store.Read<Run>(), nameof(Run.Steps)).Where(r => lastIds.Contains(r.Id)), ct);
        var bySession = (await RunViews.BuildAsync(store, runs, ct)).Where(r => r.SubjectId is not null).ToDictionary(r => r.SubjectId!.Value);

        return views.Select(v => new RemediationMonitorRow(v, bySession.GetValueOrDefault(v.Id))).ToList();
    }
}
