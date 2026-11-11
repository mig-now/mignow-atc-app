using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Runs.Get;

public sealed class GetRunHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetRunQuery, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(GetRunQuery query, CancellationToken ct)
    {
        var run = await RunLoader.LoadAsync(store, l, query.Id, false, ct);
        return run.IsFailure ? run.Error : await RunViews.BuildAsync(store, run.Value, ct);
    }
}
