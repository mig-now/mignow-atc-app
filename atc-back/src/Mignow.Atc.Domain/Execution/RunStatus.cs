namespace Mignow.Atc.Domain.Execution;

public enum RunStatus
{
    Queued,
    Running,
    Cancelling,
    Cancelled,
    Completed,
    Failed,
}
