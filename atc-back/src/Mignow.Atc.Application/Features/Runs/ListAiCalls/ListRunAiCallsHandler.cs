using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Runs.ListAiCalls;

public sealed class ListRunAiCallsHandler(IDataStore store, IStringLocalizer<Errors> l)
    : IQueryHandler<ListRunAiCallsQuery, Result<PagedResult<RunAiCallResponse>>>
{
    public async Task<Result<PagedResult<RunAiCallResponse>>> Handle(ListRunAiCallsQuery query, CancellationToken ct)
    {
        var run = await RunLoader.LoadAsync(store, l, query.RunId, false, ct);
        if (run.IsFailure)
            return run.Error;
        var runId = run.Value.Id;
        var projected = store.Read<RunAiCall>().Where(c => c.RunId == runId).OrderBy(c => c.Id)
            .Select(c => new RunAiCallResponse(c.Id, c.Usage, c.Model, c.InputTokens, c.OutputTokens, c.Cost, c.Ok, c.ErrorCode, c.ItemRef, c.At));
        return await store.PageAsync(projected, query.Page, query.PageSize, ct);
    }
}
