using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Execution;

/// <summary>
/// Uma execução ativa por ambiente. Esta conferência dá o 409 amigável com o run que bloqueia (em
/// <see cref="Error.Data"/>); a garantia contra corrida é o índice único parcial no banco (também 409).
/// </summary>
public static class RunLock
{
    public static async Task<Result> EnsureFreeAsync(IDataStore store, int sapSystemId, IStringLocalizer<Errors> l, CancellationToken ct)
    {
        var active = Run.ActiveStatuses;
        var busy = await store.FirstOrDefaultAsync(
            store.Including(store.Read<Run>(), nameof(Run.Steps)).Where(r => r.SapSystemId == sapSystemId && active.Contains(r.Status)), ct);
        if (busy is null)
            return Result.Success();
        var view = await RunViews.BuildAsync(store, busy, ct);
        var error = l.Conflict("Run.EnvironmentBusy", busy.Kind.ToString(), busy.StartedBy);
        return Result.Failure(error with { Data = view });
    }
}
