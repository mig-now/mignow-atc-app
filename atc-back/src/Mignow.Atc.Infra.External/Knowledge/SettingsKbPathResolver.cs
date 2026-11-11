using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mignow.Atc.Application.Features.Settings;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Infra.External.Knowledge;

/// <summary>
/// Lê <c>GeneralSettings.KbPath</c> (decisão 16) com cache curto; sem banco (ou falha), usa <c>Knowledge:KbPath</c> da
/// configuração e, por fim, o padrão. <c>Knowledge:RepoRoot</c> força a raiz do repositório.
/// </summary>
public sealed class SettingsKbPathResolver(IServiceScopeFactory scopes, IConfiguration configuration, ILogger<SettingsKbPathResolver> logger) : IKbPathResolver
{
    private static readonly TimeSpan CacheFor = TimeSpan.FromSeconds(10);
    private readonly object _gate = new();
    private string? _cached;
    private DateTime _cachedAt;

    public async Task<string> ResolveRootAsync(CancellationToken ct)
    {
        lock (_gate)
        {
            if (_cached is not null && DateTime.UtcNow - _cachedAt < CacheFor)
                return _cached;
        }
        var configured = await ReadConfiguredAsync(ct);
        var root = KbPaths.Resolve(configured, configuration["Knowledge:RepoRoot"]);
        lock (_gate)
        {
            _cached = root;
            _cachedAt = DateTime.UtcNow;
        }
        return root;
    }

    private async Task<string> ReadConfiguredAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var store = scope.ServiceProvider.GetRequiredService<IDataStore>();
            var settings = await SettingsStore.LoadAsync(store, false, ct);
            if (!string.IsNullOrWhiteSpace(settings.KbPath))
                return settings.KbPath;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Não foi possível ler KbPath das configurações; usando a configuração/padrão.");
        }
        return configuration["Knowledge:KbPath"] ?? GeneralSettings.DefaultKbPath;
    }
}
