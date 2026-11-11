using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.AiProviders.List;

public sealed class ListAiProvidersHandler(IDataStore store) : IQueryHandler<ListAiProvidersQuery, Result<IReadOnlyList<AiProviderResponse>>>
{
    public async Task<Result<IReadOnlyList<AiProviderResponse>>> Handle(ListAiProvidersQuery query, CancellationToken ct)
    {
        var list = await store.ListAsync(store.Read<AiProvider>().OrderByDescending(p => p.IsDefault).ThenBy(p => p.Name), ct);
        return list.Select(p => p.ToResponse()).ToList();
    }
}
