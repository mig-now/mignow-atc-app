using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.Settings;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Executa uma execução já reservada (Running): passo a passo, pulando os concluídos (retomada/retry), parando num
/// ponto seguro se o consultor interromper. Nunca deixa a execução presa em Running: erro inesperado vira Failed.
/// </summary>
public sealed class RunProcessor(IServiceScopeFactory scopes, RunPipelineCatalog catalog, IOptions<RunWorkerOptions> options, ILogger<RunProcessor> logger)
{
    public async Task ProcessAsync(int runId, CancellationToken hostCt)
    {
        await using var scope = scopes.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var store = sp.GetRequiredService<IDataStore>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
        var clock = sp.GetRequiredService<IDateTimeProvider>();
        var notifier = sp.GetRequiredService<IRunNotifier>();

        var run = await store.FirstOrDefaultAsync(store.Including(store.Query<Run>(), nameof(Run.Steps)).Where(r => r.Id == runId), CancellationToken.None);
        if (run is null)
            return;
        var pipeline = catalog.For(run.Kind);
        try
        {
            if (pipeline is null)
            {
                run.Fail(clock.UtcNow, "Run.NoPipeline", Json.Args(("kind", run.Kind)));
                await unitOfWork.SaveChangesAsync(CancellationToken.None);
                await notifier.RunChangedAsync(run.Guid, CancellationToken.None);
                return;
            }
            var systemId = run.SapSystemId;
            var system = await store.FirstOrDefaultAsync(store.Including(store.Query<SapSystem>(), nameof(SapSystem.Customer)).Where(s => s.Id == systemId),
                CancellationToken.None);
            var connection = await sp.GetRequiredService<SapConnections>().ForAsync(system!, CancellationToken.None,
                trustOnFirstUse: run.Kind == RunKind.ConnectionTest);
            if (connection.IsFailure)
            {
                run.Fail(clock.UtcNow, connection.Error.Code);
                await unitOfWork.SaveChangesAsync(CancellationToken.None);
                await notifier.RunChangedAsync(run.Guid, CancellationToken.None);
                return;
            }
            var settings = await SettingsStore.LoadAsync(store, false, CancellationToken.None);
            var context = new RunContext(run, system!, system!.Customer, connection.Value, settings, store, unitOfWork, notifier, clock,
                TimeSpan.FromMilliseconds(Math.Max(0, options.Value.ProgressSaveMilliseconds)), hostCt);
            await context.LogAsync(EventLevel.Info, run.StartedAt == run.HeartbeatAt ? "Run.Started" : "Run.Resumed");
            await context.RunChangedAsync();

            foreach (var stepType in pipeline.StepTypes)
            {
                var step = (IRunStep)sp.GetRequiredService(stepType);
                var state = run.Step(step.Code);
                if (state.Status is StepStatus.Completed or StepStatus.Skipped)
                    continue; // retomada: o que já foi feito não roda de novo
                if (await context.IsCancellationRequestedAsync())
                    break;
                context.CurrentStep = step.Code;
                state.Start(clock.UtcNow);
                await context.SaveAsync();
                await context.StepChangedAsync(step.Code);

                var outcome = await step.ExecuteAsync(context);
                if (outcome.Cancelled)
                    break;
                if (outcome.StopRun)
                {
                    state.Fail(clock.UtcNow, outcome.MessageCode ?? "Step.Failed", outcome.MessageArgs);
                    run.Fail(clock.UtcNow, outcome.MessageCode ?? "Step.Failed", outcome.MessageArgs);
                    await context.LogAsync(EventLevel.Error, outcome.MessageCode ?? "Step.Failed", outcome.MessageArgs);
                    await context.StepChangedAsync(step.Code);
                    await context.RunChangedAsync();
                    return;
                }
                state.Complete(clock.UtcNow, outcome.MessageCode, outcome.MessageArgs);
                await context.SaveAsync();
                await context.StepChangedAsync(step.Code);
            }

            if (await context.IsCancellationRequestedAsync())
            {
                run.MarkCancelled(clock.UtcNow);
                await context.LogAsync(EventLevel.Warning, "Run.Cancelled", Json.Args(("user", run.CancelledBy)));
                await context.RunChangedAsync();
                return;
            }
            run.Complete(clock.UtcNow);
            await context.LogAsync(EventLevel.Info, "Run.Completed");
            await context.RunChangedAsync();
        }
        catch (OperationCanceledException) when (hostCt.IsCancellationRequested)
        {
            // Parada do processo: a execução fica Running e a recuperação a devolve para a fila no próximo início.
            logger.LogWarning("Execução {RunId} interrompida pela parada do processo; será retomada.", runId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Execução {RunId} falhou com erro inesperado", runId);
            try
            {
                // A alteração que causou o erro ainda está pendente: descarta e recarrega, senão a falha também não salva.
                store.DiscardChanges();
                run = await store.FirstOrDefaultAsync(store.Including(store.Query<Run>(), nameof(Run.Steps)).Where(r => r.Id == runId), CancellationToken.None)
                      ?? run;
                foreach (var s in run.Steps.Where(s => s.Status == StepStatus.Processing))
                    s.Fail(clock.UtcNow, "Step.UnexpectedError");
                run.Fail(clock.UtcNow, "Run.UnexpectedError", Json.Args(("type", ex.GetType().Name)));
                store.Add(RunEvent.Create(run.Id, clock.UtcNow, EventLevel.Error, null, "Run.UnexpectedError", Json.Args(("type", ex.GetType().Name))));
                await unitOfWork.SaveChangesAsync(CancellationToken.None);
                await notifier.RunChangedAsync(run.Guid, CancellationToken.None);
            }
            catch (Exception inner)
            {
                logger.LogError(inner, "Não foi possível marcar a execução {RunId} como falha", runId);
            }
        }
    }
}
