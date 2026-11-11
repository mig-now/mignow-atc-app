namespace Mignow.Atc.Application.Engine.Running;

/// <summary>Seção <c>Engine</c>.</summary>
public sealed class RunWorkerOptions
{
    public const string Section = "Engine";

    public int PollSeconds { get; set; } = 3;

    /// <summary>Execuções simultâneas por instância (cada uma num ambiente diferente, pelo lock por ambiente).</summary>
    public int MaxConcurrentRuns { get; set; } = 6;

    /// <summary>Sem heartbeat há mais que isso, a execução é considerada órfã e volta para a fila.</summary>
    public int OrphanAfterSeconds { get; set; } = 300;

    /// <summary>Intervalo mínimo entre gravações de progresso (o heartbeat vai junto).</summary>
    public int ProgressSaveMilliseconds { get; set; } = 1000;

    public bool Enabled { get; set; } = true;
}
