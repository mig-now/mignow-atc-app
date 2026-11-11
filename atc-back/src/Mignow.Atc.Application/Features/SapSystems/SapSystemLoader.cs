using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems;

/// <summary>Ambiente pelo id público, com o cliente.</summary>
public static class SapSystemLoader
{
    public static Task<SapSystem?> LoadAsync(IDataStore store, Guid id, bool tracked, CancellationToken ct)
    {
        var q = tracked ? store.Query<SapSystem>() : store.Read<SapSystem>();
        return store.FirstOrDefaultAsync(store.Including(q, nameof(SapSystem.Customer)).Where(s => s.Guid == id), ct);
    }
}
