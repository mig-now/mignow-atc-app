using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Exemptions.Check;

public sealed class StartExemptionCheckHandler(ExemptionRunStarter starter) : ICommandHandler<StartExemptionCheckCommand, Result<RunResponse>>
{
    public Task<Result<RunResponse>> Handle(StartExemptionCheckCommand command, CancellationToken ct) =>
        starter.StartAsync(RunKind.ExemptionCheck, command.BatchId, null, ct);
}
