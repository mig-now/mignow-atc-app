using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Dtos.Runs;

/// <summary>Monta as respostas das execuções (nomes do cadastro). As execuções precisam vir com os passos (Include).</summary>
public static class RunViews
{
    public static async Task<IReadOnlyList<RunResponse>> BuildAsync(IDataStore store, IReadOnlyList<Run> runs, CancellationToken ct)
    {
        if (runs.Count == 0)
            return [];
        var systemIds = runs.Select(r => r.SapSystemId).Distinct().ToArray();
        var systems = (await store.ListAsync(store.Including(store.Read<SapSystem>(), nameof(SapSystem.Customer)).Where(s => systemIds.Contains(s.Id)), ct))
            .ToDictionary(s => s.Id);
        return [.. runs.Select(r =>
        {
            var s = systems[r.SapSystemId];
            return new RunResponse(r.Guid, r.Kind, r.Status, s.Customer.Guid, s.Customer.Name, s.Guid, s.DisplayName, r.SubjectId,
                RunMappings.Element(r.ParametersJson), RunMappings.Element(r.ResultJson), r.StartedBy, r.CancelledBy, r.QueuedAt, r.StartedAt,
                r.FinishedAt, r.HeartbeatAt, r.FailureCode, RunMappings.Args(r.FailureArgs), r.Parallelism, r.AiCalls, r.AiInputTokens,
                r.AiOutputTokens, r.AiCost, r.AiCallsWithoutCost, [.. r.Steps.OrderBy(x => x.Order).Select(x => x.ToResponse())]);
        })];
    }

    public static async Task<RunResponse> BuildAsync(IDataStore store, Run run, CancellationToken ct) => (await BuildAsync(store, [run], ct))[0];
}
