using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Running;

/// <summary>Execuções órfãs (processo caiu, sem heartbeat): voltam para a fila no último passo concluído.</summary>
public sealed class RunRecovery(IServiceScopeFactory scopes, IOptions<RunWorkerOptions> options, ILogger<RunRecovery> logger)
{
    public async Task RecoverAsync(bool onStartup, CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IDataStore>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var limit = clock.UtcNow.AddSeconds(-options.Value.OrphanAfterSeconds);
        var q = store.Query<Run>().Where(r => r.Status == RunStatus.Running || r.Status == RunStatus.Cancelling);
        if (!onStartup)
            q = q.Where(r => r.HeartbeatAt == null || r.HeartbeatAt < limit);
        var orphans = await store.ListAsync(store.Including(q, nameof(Run.Steps)), ct);
        foreach (var run in orphans)
        {
            // Interrupção pedida e o processo caiu antes de concluir: fecha como cancelada.
            if (run.Status == RunStatus.Cancelling)
                run.MarkCancelled(clock.UtcNow);
            else
                run.Requeue();
            store.Add(RunEvent.Create(run.Id, clock.UtcNow, EventLevel.Warning, null, "Run.Recovered", null));
            logger.LogWarning("Execução {RunId} ({Kind}) sem heartbeat: devolvida para a fila.", run.Id, run.Kind);
        }
        if (orphans.Count > 0)
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(ct);
    }
}
