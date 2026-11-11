using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Extractions.Get;

public sealed class GetExtractionHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetExtractionQuery, Result<ExtractionResponse>>
{
    public async Task<Result<ExtractionResponse>> Handle(GetExtractionQuery query, CancellationToken ct)
    {
        var extraction = await store.FirstOrDefaultAsync(store.Read<AtcExtraction>().Where(e => e.Guid == query.Id && e.IsComplete), ct);
        if (extraction is null)
            return l.NotFound("Extraction.NotFound");
        var system = (await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(s => s.Id == extraction.SapSystemId), ct))!;
        return extraction.ToResponse(system);
    }
}
