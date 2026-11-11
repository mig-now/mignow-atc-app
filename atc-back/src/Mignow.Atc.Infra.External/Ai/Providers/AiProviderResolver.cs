using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Infra.External.Ai.Providers;

/// <summary>
/// Lê o provedor padrão do cadastro (⚙ → Provedores IA): um só modelo para todos os usos. Escopo próprio: o motor pode chamar em paralelo e o
/// DbContext da execução não pode ser compartilhado. Cache curto (1 min).
/// </summary>
public sealed class AiProviderResolver(IServiceScopeFactory scopes)
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(1);
    // Referência (troca atômica): chamadas paralelas do motor leem enquanto outra grava.
    private volatile CacheEntry? _cache;

    public async Task<ResolvedAiProvider?> DefaultAsync(CancellationToken ct)
    {
        if (_cache is { } cached && DateTimeOffset.UtcNow - cached.At < Ttl)
            return cached.Provider;
        await using var scope = scopes.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IDataStore>();
        var p = await store.FirstOrDefaultAsync(store.Read<AiProvider>().Where(x => x.Active && x.IsDefault), ct);
        ResolvedAiProvider? resolved = null;
        if (p is not null)
        {
            var key = p.ApiKeySecretRef is null ? null : await scope.ServiceProvider.GetRequiredService<ISecretStore>().GetAsync(p.ApiKeySecretRef, ct);
            resolved = new ResolvedAiProvider(p.Kind, p.Model, key);
        }
        _cache = new CacheEntry(DateTimeOffset.UtcNow, resolved);
        return resolved;
    }

    /// <summary>Depois de alterar o cadastro, a próxima chamada lê de novo.</summary>
    public void Invalidate() => _cache = null;

    private sealed record CacheEntry(DateTimeOffset At, ResolvedAiProvider? Provider);
}
