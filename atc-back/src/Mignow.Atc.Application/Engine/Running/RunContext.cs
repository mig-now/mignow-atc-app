using System.Text.Json;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Estado de uma execução em andamento. O DbContext do escopo não é thread-safe e os passos podem trabalhar em paralelo:
/// TODO acesso ao banco passa por <see cref="WithDbAsync{T}"/> / <see cref="MutateAsync"/> (um por vez).
/// </summary>
public sealed class RunContext(
    Run run, SapSystem system, Customer customer, SapConnection connection, GeneralSettings settings,
    IDataStore store, IUnitOfWork unitOfWork, IRunNotifier notifier, IDateTimeProvider clock, TimeSpan progressInterval, CancellationToken ct)
{
    private readonly SemaphoreSlim _db = new(1, 1);
    private DateTimeOffset _lastProgressSave = DateTimeOffset.MinValue;

    public Run Run { get; } = run;

    public SapSystem SapSystem { get; } = system;

    public Customer Customer { get; } = customer;

    public SapConnection SapConnection { get; } = connection;

    public GeneralSettings Settings { get; } = settings;

    public IDataStore Store { get; } = store;

    public IDateTimeProvider Clock { get; } = clock;

    /// <summary>Parada do processo (host). A interrupção pedida pelo usuário é <see cref="IsCancellationRequestedAsync"/>.</summary>
    public CancellationToken Ct { get; } = ct;

    public string? CurrentStep { get; internal set; }

    /// <summary>Parâmetros do run (<see cref="Run.ParametersJson"/>) desserializados.</summary>
    public T Parameters<T>() where T : class =>
        JsonSerializer.Deserialize<T>(Run.ParametersJson, Json.Options) ?? throw new InvalidOperationException($"Parâmetros inválidos para {typeof(T).Name}.");

    public async Task<T> WithDbAsync<T>(Func<Task<T>> action)
    {
        await _db.WaitAsync(CancellationToken.None);
        try
        {
            return await action();
        }
        finally
        {
            _db.Release();
        }
    }

    public Task WithDbAsync(Func<Task> action) => WithDbAsync(async () => { await action(); return true; });

    /// <summary>Altera entidades rastreadas sob a mesma trava do SaveChanges (uma alteração no meio de um save se perderia).</summary>
    public Task MutateAsync(Action change) => WithDbAsync(() =>
    {
        change();
        return Task.CompletedTask;
    });

    /// <summary>Grava o que mudou (com o heartbeat da execução).</summary>
    public Task SaveAsync() => WithDbAsync(SaveUnlockedAsync);

    private async Task SaveUnlockedAsync()
    {
        Run.Heartbeat(Clock.UtcNow);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    public Task LogAsync(EventLevel level, string code, string? args = null, string? itemRef = null) =>
        WithDbAsync(async () =>
        {
            Store.Add(RunEvent.Create(Run.Id, Clock.UtcNow, level, CurrentStep, code, args, itemRef));
            await SaveUnlockedAsync();
        });

    /// <summary>
    /// Progresso do passo atual (<c>done/total</c>). Gravado e notificado no máximo a cada
    /// <c>Engine:ProgressSaveMilliseconds</c> (sempre no último item).
    /// </summary>
    public async Task ReportProgressAsync(int done, int total)
    {
        if (CurrentStep is null)
            return;
        var now = Clock.UtcNow;
        var save = false;
        await MutateAsync(() =>
        {
            Run.Step(CurrentStep).ReportProgress(done, total);
            if (done >= total || now - _lastProgressSave >= progressInterval)
            {
                _lastProgressSave = now;
                save = true;
            }
        });
        if (!save)
            return;
        await SaveAsync();
        await StepChangedAsync(CurrentStep);
    }

    /// <summary>Resumo do run (contagens): serializado em <see cref="Run.ResultJson"/>.</summary>
    public Task SetResultAsync(object result) => MutateAsync(() => Run.SetResult(Json.Write(result)));

    /// <summary>
    /// Acrescenta/atualiza só as chaves de <paramref name="partial"/> no resumo do run, mantendo as dos passos anteriores. Usado pelos passos que
    /// convivem no mesmo run (ex.: proposta + aplicação + relatório do lote, remediação automática) para um não apagar o resultado do outro.
    /// </summary>
    public Task MergeResultAsync(object partial) => MutateAsync(() =>
    {
        var current = string.IsNullOrWhiteSpace(Run.ResultJson) ? null : System.Text.Json.Nodes.JsonNode.Parse(Run.ResultJson) as System.Text.Json.Nodes.JsonObject;
        current ??= [];
        if (System.Text.Json.Nodes.JsonNode.Parse(Json.Write(partial)) is System.Text.Json.Nodes.JsonObject add)
            foreach (var (key, value) in add.ToList())
            {
                add.Remove(key);
                current[key] = value;
            }
        Run.SetResult(current.ToJsonString(Json.Options));
    });

    public Task SetParallelismAsync(int parallelism) => MutateAsync(() => Run.SetParallelism(parallelism));

    public Task StepChangedAsync(string code) => SafeNotify(() => notifier.StepChangedAsync(Run.Guid, code, CancellationToken.None));

    public Task RunChangedAsync() => SafeNotify(() => notifier.RunChangedAsync(Run.Guid, CancellationToken.None));

    /// <summary>O consultor pediu para interromper? Lido do banco (o pedido vem de outra requisição).</summary>
    public Task<bool> IsCancellationRequestedAsync() =>
        WithDbAsync(async () =>
        {
            var runId = Run.Id;
            var status = await Store.FirstOrDefaultAsync(Store.Read<Run>().Where(r => r.Id == runId).Select(r => (RunStatus?)r.Status), CancellationToken.None);
            return status is RunStatus.Cancelling or RunStatus.Cancelled || Ct.IsCancellationRequested;
        });

    /// <summary>
    /// Registra uma chamada de IA (inclusive as que falharam): soma no total da execução e guarda a chamada. Gravado no
    /// próximo SaveChanges. Uso típico: <c>gateway.CompleteAsync(req, u =&gt; ctx.AddAiUsage(u, req.Usage, item), ct)</c>.
    /// </summary>
    public void AddAiUsage(AiUsageInfo usage, AiUsage purpose, string? itemRef = null)
    {
        _db.Wait();
        try
        {
            Run.AddAiUsage(usage.InputTokens, usage.OutputTokens, usage.Cost);
            Store.Add(RunAiCall.Create(Run.Id, purpose, usage.Model, usage.InputTokens, usage.OutputTokens, usage.Cost, usage.Ok, usage.ErrorCode,
                itemRef, Clock.UtcNow));
        }
        finally
        {
            _db.Release();
        }
    }

    private static async Task SafeNotify(Func<Task> notify)
    {
        try
        {
            await notify();
        }
        catch (Exception)
        {
            // Notificação ao vivo é melhor esforço: o front também faz polling.
        }
    }
}
