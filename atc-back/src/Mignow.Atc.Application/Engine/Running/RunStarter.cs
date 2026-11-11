using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Execution;
using Mignow.Atc.Application.Ports.Identity;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Enfileira uma execução (usado pelos command handlers dos módulos): confere o lock do ambiente (409 amigável com o run
/// que bloqueia), monta os passos do pipeline do <see cref="RunKind"/> e adiciona ao store — o commit é do
/// TransactionDecorator; o índice único parcial barra a corrida (também 409).
/// </summary>
public sealed class RunStarter(IDataStore store, RunPipelineCatalog catalog, IServiceProvider scope, ICurrentUser user, IDateTimeProvider clock,
    IStringLocalizer<Errors> l)
{
    public async Task<Result<Run>> EnqueueAsync(RunKind kind, SapSystem system, Guid? subjectId, object? parameters, CancellationToken ct)
    {
        if (catalog.For(kind) is null)
            return l.Conflict("Run.NoPipeline", kind.ToString());
        if (!system.Active)
            return l.Conflict("SapSystem.Inactive", system.DisplayName);
        var free = await RunLock.EnsureFreeAsync(store, system.Id, l, ct);
        if (free.IsFailure)
            return free.Error;
        var run = Run.Create(kind, system.CustomerId, system.Id, subjectId, parameters is null ? null : Json.Write(parameters), user.Actor,
            catalog.StepCodes(kind, scope), clock.UtcNow);
        store.Add(run);
        return run;
    }
}
