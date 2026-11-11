using Microsoft.AspNetCore.SignalR;
using Mignow.Atc.Application.Ports.Execution;

namespace Mignow.Atc.Api.Realtime;

public sealed class SignalRRunNotifier(IHubContext<RunHub> hub) : IRunNotifier
{
    public Task RunChangedAsync(Guid runId, CancellationToken ct) =>
        hub.Clients.Group(RunHub.Group(runId)).SendAsync("RunChanged", runId, ct);

    public Task StepChangedAsync(Guid runId, string stepCode, CancellationToken ct) =>
        hub.Clients.Group(RunHub.Group(runId)).SendAsync("StepChanged", runId, stepCode, ct);
}
