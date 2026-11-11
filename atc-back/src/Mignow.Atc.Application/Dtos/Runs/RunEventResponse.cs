using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Dtos.Runs;

public sealed record RunEventResponse(long Id, DateTimeOffset At, EventLevel Level, string? StepCode, string Code,
    IReadOnlyDictionary<string, string?>? Args, string? ItemRef);
