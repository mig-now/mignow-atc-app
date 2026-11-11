using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Reports.History;

public sealed class GetHistoryHandler(IDataStore store) : IQueryHandler<GetHistoryQuery, Result<IReadOnlyList<ProcessHistoryEntry>>>
{
    public async Task<Result<IReadOnlyList<ProcessHistoryEntry>>> Handle(GetHistoryQuery query, CancellationToken ct)
    {
        var module = Enum.Parse<ProcessModule>(query.Module, true);
        var q = store.Read<Run>();
        if (query.SapSystemId is not null)
        {
            var id = await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(s => s.Guid == query.SapSystemId).Select(s => (int?)s.Id), ct);
            q = q.Where(r => r.SapSystemId == (id ?? -1));
        }
        if (query.CustomerId is not null)
        {
            var id = await store.FirstOrDefaultAsync(store.Read<Customer>().Where(c => c.Guid == query.CustomerId).Select(c => (int?)c.Id), ct);
            q = q.Where(r => r.CustomerId == (id ?? -1));
        }
        var runs = await store.ListAsync(q.OrderBy(r => r.QueuedAt), ct);
        var ids = runs.Select(r => r.Id).ToList();
        var calls = await store.ListAsync(store.Read<RunAiCall>().Where(c => ids.Contains(c.RunId)), ct);
        var data = runs.Select(r => new RunWithCalls(r, calls.Where(c => c.RunId == r.Id).ToList()));
        return Result.Success(ProcessHistoryBuilder.Build(data, module));
    }
}
