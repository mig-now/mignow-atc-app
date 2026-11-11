using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Ports.Execution;

namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Worker hospedado na API: reserva execuções da fila (Postgres, SKIP LOCKED) e processa até
/// <see cref="RunWorkerOptions.MaxConcurrentRuns"/> ao mesmo tempo. Na partida (e a cada minuto) recupera as órfãs.
/// </summary>
public sealed class RunWorker(IServiceScopeFactory scopes, RunProcessor processor, RunRecovery recovery, IOptions<RunWorkerOptions> options,
    ILogger<RunWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;
        var slots = new SemaphoreSlim(Math.Max(1, options.Value.MaxConcurrentRuns));
        var running = new List<Task>();
        var lastRecovery = DateTimeOffset.MinValue;
        try
        {
            await recovery.RecoverAsync(onStartup: true, stoppingToken);
            lastRecovery = DateTimeOffset.UtcNow;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Recuperação inicial das execuções falhou");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (DateTimeOffset.UtcNow - lastRecovery > TimeSpan.FromMinutes(1))
                {
                    await recovery.RecoverAsync(onStartup: false, stoppingToken);
                    lastRecovery = DateTimeOffset.UtcNow;
                }
                running.RemoveAll(t => t.IsCompleted);
                while (slots.CurrentCount > 0)
                {
                    int? runId;
                    await using (var scope = scopes.CreateAsyncScope())
                        runId = await scope.ServiceProvider.GetRequiredService<IRunQueue>().ClaimNextAsync(stoppingToken);
                    if (runId is null)
                        break;
                    await slots.WaitAsync(stoppingToken);
                    var id = runId.Value;
                    running.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await processor.ProcessAsync(id, stoppingToken);
                        }
                        finally
                        {
                            slots.Release();
                        }
                    }, CancellationToken.None));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha no laço do worker de execuções");
            }
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.Value.PollSeconds)), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        await Task.WhenAll(running);
    }
}
