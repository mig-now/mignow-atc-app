using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Settings;

public static class SettingsStore
{
    /// <summary>A linha única de parâmetros (o seed cria; se faltar, valores padrão em memória).</summary>
    public static async Task<GeneralSettings> LoadAsync(IDataStore store, bool tracked, CancellationToken ct) =>
        await store.FirstOrDefaultAsync((tracked ? store.Query<GeneralSettings>() : store.Read<GeneralSettings>()).OrderBy(s => s.Id), ct)
        ?? GeneralSettings.CreateDefault();
}
