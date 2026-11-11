using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Exemptions.Simulate;

public sealed class StartExemptionSimulateHandler(ExemptionRunStarter starter) : ICommandHandler<StartExemptionSimulateCommand, Result<RunResponse>>
{
    public Task<Result<RunResponse>> Handle(StartExemptionSimulateCommand command, CancellationToken ct) =>
        starter.StartAsync(RunKind.ExemptionSimulate, command.BatchId, command.Seq, ct);
}
