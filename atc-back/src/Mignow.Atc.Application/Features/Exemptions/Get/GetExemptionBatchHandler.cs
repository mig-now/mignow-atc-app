using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Exemptions.Get;

public sealed class GetExemptionBatchHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetExemptionBatchQuery, Result<ExemptionBatchResponse>>
{
    public async Task<Result<ExemptionBatchResponse>> Handle(GetExemptionBatchQuery query, CancellationToken ct)
    {
        var batch = await ExemptionBatchLoader.LoadAsync(store, l, query.Id, false, false, ct);
        return batch.IsFailure ? batch.Error : await ExemptionViews.BatchAsync(store, batch.Value, ct);
    }
}
