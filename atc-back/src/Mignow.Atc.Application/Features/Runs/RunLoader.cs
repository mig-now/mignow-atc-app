using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Runs;

/// <summary>Carrega uma execução (com os passos) pelo id público.</summary>
internal static class RunLoader
{
    public static async Task<Result<Run>> LoadAsync(IDataStore store, IStringLocalizer<Errors> l, Guid runId, bool tracked, CancellationToken ct)
    {
        var source = tracked ? store.Query<Run>() : store.Read<Run>();
        var run = await store.FirstOrDefaultAsync(store.Including(source, nameof(Run.Steps)).Where(r => r.Guid == runId), ct);
        return run is null ? l.NotFound("Run.NotFound") : run;
    }
}
