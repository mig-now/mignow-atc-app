using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Runs.ListEvents;

public sealed class ListRunEventsHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<ListRunEventsQuery, Result<PagedResult<RunEventResponse>>>
{
    public async Task<Result<PagedResult<RunEventResponse>>> Handle(ListRunEventsQuery query, CancellationToken ct)
    {
        var run = await RunLoader.LoadAsync(store, l, query.RunId, false, ct);
        if (run.IsFailure)
            return run.Error;
        var runId = run.Value.Id;
        var q = store.Read<RunEvent>().Where(e => e.RunId == runId);
        if (query.Level is not null)
            q = q.Where(e => e.Level == query.Level);
        if (!string.IsNullOrWhiteSpace(query.ItemRef))
            q = q.Where(e => e.ItemRef == query.ItemRef);
        var page = await store.PageAsync(q.OrderByDescending(e => e.Id), query.Page, query.PageSize, ct);
        return new PagedResult<RunEventResponse>(
            [.. page.Items.Select(e => new RunEventResponse(e.Id, e.At, e.Level, e.StepCode, e.Code, RunMappings.Args(e.ArgsJson), e.ItemRef))],
            page.Page, page.PageSize, page.Total);
    }
}
