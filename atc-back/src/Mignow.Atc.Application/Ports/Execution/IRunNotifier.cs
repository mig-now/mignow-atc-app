namespace Mignow.Atc.Application.Ports.Execution;

/// <summary>Progresso ao vivo (SignalR, grupo por execução). Falha de notificação nunca derruba a execução.</summary>
public interface IRunNotifier
{
    Task RunChangedAsync(Guid runId, CancellationToken ct);

    Task StepChangedAsync(Guid runId, string stepCode, CancellationToken ct);
}
