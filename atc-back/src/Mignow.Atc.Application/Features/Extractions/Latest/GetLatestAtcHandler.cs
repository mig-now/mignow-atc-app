using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Features.SapSystems;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Sap.Extraction;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Application.Services;

namespace Mignow.Atc.Application.Features.Extractions.Latest;

public sealed class GetLatestAtcHandler(IDataStore store, SapConnections connections, IAtcExtractionReader reader, IStringLocalizer<Errors> l)
    : IQueryHandler<GetLatestAtcQuery, Result<AtcLatestResponse>>
{
    public async Task<Result<AtcLatestResponse>> Handle(GetLatestAtcQuery query, CancellationToken ct)
    {
        var system = await SapSystemLoader.LoadAsync(store, query.SapSystemId, false, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var connection = await connections.ForAsync(system, ct);
        if (connection.IsFailure)
            return connection.Error;
        var selection = await AtcLatestFinder.SelectAsync(reader, l, connection.Value, ct);
        if (selection.IsFailure)
            return selection.Error;
        var existing = await ExtractionQueries.FindByDisplayIdAsync(store, system.Id, selection.Value.Latest!.DisplayId, false, ct);
        return AtcLatestFinder.Describe(selection.Value, system, existing);
    }
}
