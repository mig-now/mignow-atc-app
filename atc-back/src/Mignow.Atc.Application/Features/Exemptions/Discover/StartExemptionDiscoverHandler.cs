using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Features.Exemptions.Discover;

public sealed class StartExemptionDiscoverHandler(ExemptionRunStarter starter) : ICommandHandler<StartExemptionDiscoverCommand, Result<RunResponse>>
{
    public Task<Result<RunResponse>> Handle(StartExemptionDiscoverCommand command, CancellationToken ct) =>
        starter.StartAsync(RunKind.ExemptionDiscover, command.BatchId, null, ct);
}
