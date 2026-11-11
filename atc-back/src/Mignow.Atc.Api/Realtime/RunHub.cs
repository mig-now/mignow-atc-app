using Microsoft.AspNetCore.SignalR;

namespace Mignow.Atc.Api.Realtime;

/// <summary>Progresso ao vivo: o front entra no grupo da execução e recebe <c>RunChanged</c>/<c>StepChanged</c>.</summary>
public sealed class RunHub : Hub
{
    public const string Path = "/hubs/runs";

    public Task JoinRun(Guid runId) => Groups.AddToGroupAsync(Context.ConnectionId, Group(runId));

    public Task LeaveRun(Guid runId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(runId));

    public static string Group(Guid runId) => $"run:{runId}";
}
