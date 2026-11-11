using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Ponte entre os serviços do módulo e a execução (<see cref="RunContext"/>): alterações em entidades sob a trava do banco, gravação,
/// registro de IA, interrupção, eventos e progresso. Os serviços dependem só deste tipo, então rodam também sem banco (testes).
/// </summary>
public sealed class EngineHost(
    SapConnection connection, GeneralSettings settings, Func<Action, Task> mutate, Func<Task> save, Action<AiUsageInfo, AiUsage, string?> addUsage,
    Func<Task<bool>> isCancelled, Func<EventLevel, string, string?, string?, Task> log, Func<int, int, Task> progress, CancellationToken ct)
{
    public SapConnection Connection { get; } = connection;

    public GeneralSettings Settings { get; } = settings;

    public CancellationToken Ct { get; } = ct;

    public Task MutateAsync(Action change) => mutate(change);

    public Task SaveAsync() => save();

    public void AddAiUsage(AiUsageInfo usage, AiUsage purpose, string? itemRef) => addUsage(usage, purpose, itemRef);

    public Task<bool> IsCancelledAsync() => isCancelled();

    public Task LogAsync(EventLevel level, string code, string? args = null, string? itemRef = null) => log(level, code, args, itemRef);

    public Task ProgressAsync(int done, int total) => progress(done, total);

    public static EngineHost From(RunContext ctx) => new(ctx.SapConnection, ctx.Settings, ctx.MutateAsync, ctx.SaveAsync, ctx.AddAiUsage,
        ctx.IsCancellationRequestedAsync, ctx.LogAsync, ctx.ReportProgressAsync, ctx.Ct);

    /// <summary>Sem execução (testes): alterações direto, sem gravação, registro de IA numa lista.</summary>
    public static EngineHost Standalone(SapConnection connection, GeneralSettings settings, List<(AiUsageInfo Usage, AiUsage Purpose, string? ItemRef)>? usages = null,
        List<(EventLevel Level, string Code, string? Args, string? ItemRef)>? events = null, CancellationToken ct = default) =>
        new(connection, settings, a => { a(); return Task.CompletedTask; }, () => Task.CompletedTask,
            (u, p, i) => { lock (usages ?? []) usages?.Add((u, p, i)); }, () => Task.FromResult(false),
            (l, c, a, i) => { lock (events ?? []) events?.Add((l, c, a, i)); return Task.CompletedTask; }, (_, _) => Task.CompletedTask, ct);
}
