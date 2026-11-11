using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Extractions.Monitor;

/// <summary>
/// Três leituras: ambientes ativos (com cliente), a última execução <c>Extraction</c> de cada um e a última extração gravada
/// (maior id por ambiente). Ordem: cliente, ambiente.
/// </summary>
public sealed class GetExtractionMonitorHandler(IDataStore store) : IQueryHandler<GetExtractionMonitorQuery, Result<IReadOnlyList<ExtractionMonitorRow>>>
{
    public async Task<Result<IReadOnlyList<ExtractionMonitorRow>>> Handle(GetExtractionMonitorQuery query, CancellationToken ct)
    {
        var systemsQuery = store.Including(store.Read<SapSystem>(), nameof(SapSystem.Customer)).Where(s => s.Active && s.Customer.Active);
        if (query.CustomerId is { } customerId)
            systemsQuery = systemsQuery.Where(s => s.Customer.Guid == customerId);
        var systems = (await store.ListAsync(systemsQuery, ct))
            .OrderBy(s => s.Customer.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(s => s.DisplayName, StringComparer.Ordinal).ToList();
        if (systems.Count == 0)
            return Array.Empty<ExtractionMonitorRow>();
        var ids = systems.Select(s => s.Id).ToArray();

        var lastRunIds = await store.ListAsync(store.Read<Run>().Where(r => r.Kind == RunKind.Extraction && ids.Contains(r.SapSystemId))
            .GroupBy(r => r.SapSystemId).Select(g => g.Max(r => r.Id)), ct);
        var runs = await store.ListAsync(store.Including(store.Read<Run>(), nameof(Run.Steps)).Where(r => lastRunIds.Contains(r.Id)), ct);
        var runViews = (await RunViews.BuildAsync(store, runs, ct)).ToDictionary(r => r.SapSystemId);

        var lastExtractionIds = await store.ListAsync(store.Read<AtcExtraction>().Where(e => e.IsComplete && ids.Contains(e.SapSystemId))
            .GroupBy(e => e.SapSystemId).Select(g => g.Max(e => e.Id)), ct);
        var extractions = (await store.ListAsync(store.Read<AtcExtraction>().Where(e => lastExtractionIds.Contains(e.Id)), ct))
            .ToDictionary(e => e.SapSystemId);

        return systems.Select(s => new ExtractionMonitorRow(s.Guid, s.DisplayName, s.Customer.Guid, s.Customer.Name, s.IsProduction, s.IsSimulator,
            runViews.GetValueOrDefault(s.Guid), extractions.TryGetValue(s.Id, out var e) ? e.ToResponse(s) : null)).ToList();
    }
}
