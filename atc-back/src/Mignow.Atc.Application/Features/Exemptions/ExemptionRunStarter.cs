using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Exemption;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>Enfileira a execução do lote (202 com o run): confere o lote, o <c>seq</c> pedido e a etapa do fluxo, e usa o lock do ambiente.</summary>
public sealed class ExemptionRunStarter(IDataStore store, RunStarter starter, IStringLocalizer<Errors> l)
{
    public async Task<Result<RunResponse>> StartAsync(RunKind kind, Guid batchId, IReadOnlyList<int>? seq, CancellationToken ct)
    {
        var loaded = await ExemptionBatchLoader.LoadAsync(store, l, batchId, false, true, ct);
        if (loaded.IsFailure)
            return loaded.Error;
        var batch = loaded.Value;
        var picked = seq is { Count: > 0 } ? seq.Distinct().Order().ToList() : null;
        if (picked is not null && picked.Any(s => batch.Requests.All(r => r.Seq != s)))
            return l.Validation("Exemption.SeqUnknown", string.Join(", ", picked.Where(s => batch.Requests.All(r => r.Seq != s))));
        var inScope = batch.Requests.Where(r => picked is null || picked.Contains(r.Seq)).ToList();

        var rule = Check(kind, batch, inScope);
        if (rule is not null)
            return rule;

        var systemId = batch.SapSystemId;
        var system = await store.FirstOrDefaultAsync(store.Including(store.Read<SapSystem>(), nameof(SapSystem.Customer)).Where(s => s.Id == systemId), ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var run = await starter.EnqueueAsync(kind, system, batch.Guid, new ExemptionRunParameters(batch.Guid, picked), ct);
        if (run.IsFailure)
            return run.Error;
        return await RunViews.BuildAsync(store, run.Value, ct);
    }

    /// <summary>Etapas da skill: simular só depois de descobrir (política A) e com aprovador; enviar só o simulado; conferir só o pedido.</summary>
    private Error? Check(RunKind kind, ExemptionBatch batch, List<ExemptionRequest> inScope)
    {
        switch (kind)
        {
            case RunKind.ExemptionDiscover when batch.Requests.Count == 0:
                return l.Validation("Exemption.NoFindings");
            case RunKind.ExemptionSimulate:
                if (batch.Status < ExemptionBatchStatus.Discovered)
                    return l.Conflict("Exemption.DiscoveryRequired");
                if (string.IsNullOrWhiteSpace(batch.Approver))
                    return l.Validation("Exemption.ApproverRequired");
                break;
            case RunKind.ExemptionSend:
                if (batch.Status < ExemptionBatchStatus.Simulated)
                    return l.Conflict("Exemption.SimulationRequired");
                if (string.IsNullOrWhiteSpace(batch.Approver))
                    return l.Validation("Exemption.ApproverRequired");
                if (!inScope.Any(r => r.Status == ExemptionRequestStatus.Simulated))
                    return l.Conflict("Exemption.NothingToSend");
                break;
            case RunKind.ExemptionCheck when !inScope.Any(r => r.Status == ExemptionRequestStatus.Requested):
                return l.Conflict("Exemption.NothingToCheck");
        }
        return null;
    }
}
