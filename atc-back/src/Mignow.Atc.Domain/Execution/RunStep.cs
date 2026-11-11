using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Execution;

/// <summary>Passo do pipeline, com progresso <c>done/total</c> (barra da tela de execução).</summary>
public sealed class RunStep : EntityBase
{
    private RunStep() { }

    public int RunId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public int Order { get; private set; }

    public StepStatus Status { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    /// <summary>Código i18n da mensagem do passo (ex.: erro ou resumo).</summary>
    public string? MessageCode { get; private set; }

    public string? MessageArgs { get; private set; }

    public int ProgressDone { get; private set; }

    public int ProgressTotal { get; private set; }

    internal static RunStep Create(string code, int order) =>
        new() { Code = Guard.Required(code, "Run.StepCodeRequired", 40), Order = order, Status = StepStatus.Pending };

    public void Start(DateTimeOffset at)
    {
        Status = StepStatus.Processing;
        StartedAt ??= at;
        FinishedAt = null;
        MessageCode = MessageArgs = null;
    }

    public void ReportProgress(int done, int total)
    {
        ProgressTotal = Math.Max(0, total);
        ProgressDone = Math.Clamp(done, 0, ProgressTotal == 0 ? Math.Max(0, done) : ProgressTotal);
    }

    public void Complete(DateTimeOffset at, string? messageCode = null, string? args = null)
    {
        Status = StepStatus.Completed;
        FinishedAt = at;
        MessageCode = messageCode;
        MessageArgs = args;
        if (ProgressTotal > 0)
            ProgressDone = ProgressTotal;
    }

    public void Fail(DateTimeOffset at, string messageCode, string? args = null)
    {
        Status = StepStatus.Error;
        FinishedAt = at;
        MessageCode = messageCode;
        MessageArgs = args;
    }

    public void Skip(DateTimeOffset at)
    {
        if (Status is StepStatus.Completed or StepStatus.Error)
            return;
        Status = StepStatus.Skipped;
        FinishedAt = at;
    }

    /// <summary>Reprocessar: o passo com erro e os pulados pela falha voltam a pendente; os concluídos ficam.</summary>
    public void ResetForRetry()
    {
        if (Status is StepStatus.Error or StepStatus.Skipped or StepStatus.Processing)
        {
            Status = StepStatus.Pending;
            FinishedAt = null;
        }
    }

    /// <summary>Retomada depois de queda: um passo interrompido volta a pendente.</summary>
    public void ResetIfInterrupted()
    {
        if (Status == StepStatus.Processing)
            Status = StepStatus.Pending;
    }
}
