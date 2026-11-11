using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Exemptions.Send;

public sealed class StartExemptionSendHandler(ExemptionRunStarter starter) : ICommandHandler<StartExemptionSendCommand, Result<RunResponse>>
{
    public Task<Result<RunResponse>> Handle(StartExemptionSendCommand command, CancellationToken ct) =>
        starter.StartAsync(RunKind.ExemptionSend, command.BatchId, command.Seq, ct);
}
