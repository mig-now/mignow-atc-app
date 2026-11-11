using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.AiProviders;

internal static class AiProviderDefaults
{
    /// <summary>Um único padrão (vale para todos os usos): marcar este desmarca os outros.</summary>
    public static async Task ApplyAsync(IDataStore store, AiProvider provider, bool isDefault, CancellationToken ct)
    {
        if (isDefault)
        {
            var others = await store.ListAsync(store.Query<AiProvider>().Where(p => p.IsDefault && p.Id != provider.Id), ct);
            foreach (var o in others)
                o.MarkDefault(false);
        }
        provider.MarkDefault(isDefault);
    }
}
