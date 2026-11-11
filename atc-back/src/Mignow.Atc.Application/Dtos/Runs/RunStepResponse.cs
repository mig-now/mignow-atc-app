using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Dtos.Runs;

public sealed record RunStepResponse(string Code, int Order, StepStatus Status, DateTimeOffset? StartedAt, DateTimeOffset? FinishedAt,
    string? MessageCode, IReadOnlyDictionary<string, string?>? MessageArgs, int ProgressDone, int ProgressTotal);
