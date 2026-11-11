using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.SapSystems.Get;

public sealed class GetSapSystemHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetSapSystemQuery, Result<SapSystemResponse>>
{
    public async Task<Result<SapSystemResponse>> Handle(GetSapSystemQuery query, CancellationToken ct)
    {
        var system = await SapSystemLoader.LoadAsync(store, query.Id, false, ct);
        return system is null ? l.NotFound("SapSystem.NotFound") : system.ToResponse();
    }
}
