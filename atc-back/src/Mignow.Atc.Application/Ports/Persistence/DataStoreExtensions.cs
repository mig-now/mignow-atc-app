using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Persistence;

public static class DataStoreExtensions
{
    public static async Task<PagedResult<T>> PageAsync<T>(this IDataStore store, IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var total = await store.CountAsync(query, ct);
        var items = await store.ListAsync(query.Skip((page - 1) * pageSize).Take(pageSize), ct);
        return new PagedResult<T>(items, page, pageSize, total);
    }
}
