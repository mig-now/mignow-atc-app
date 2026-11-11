using System.Text.Json;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Features.Exemptions;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>Monta as respostas do módulo (ids públicos da sessão e do ambiente, totais por status, itens dos pedidos).</summary>
public static class ExemptionViews
{
    public static async Task<IReadOnlyList<ExemptionBatchResponse>> BatchesAsync(IDataStore store, IReadOnlyList<ExemptionBatch> batches, CancellationToken ct)
    {
        if (batches.Count == 0)
            return [];
        var ids = batches.Select(b => b.Id).ToArray();
        var sessionIds = batches.Select(b => b.SessionId).Distinct().ToArray();
        var systemIds = batches.Select(b => b.SapSystemId).Distinct().ToArray();
        var sessions = (await store.ListAsync(store.Read<RemediationSession>().Where(s => sessionIds.Contains(s.Id)).Select(s => new { s.Id, s.Guid }), ct))
            .ToDictionary(s => s.Id, s => s.Guid);
        var systems = (await store.ListAsync(store.Read<SapSystem>().Where(s => systemIds.Contains(s.Id)).Select(s => new { s.Id, s.Guid }), ct))
            .ToDictionary(s => s.Id, s => s.Guid);
        var requests = (await store.ListAsync(store.Read<ExemptionRequest>().Where(r => ids.Contains(r.BatchId))
            .Select(r => new { r.BatchId, r.Status, HasXml = r.RequestXml != null }), ct)).ToLookup(r => r.BatchId);
        return [.. batches.Select(b =>
        {
            var rows = requests[b.Id].Select(r => (r.Status, r.HasXml)).ToList();
            return new ExemptionBatchResponse(b.Guid, sessions.GetValueOrDefault(b.SessionId), systems.GetValueOrDefault(b.SapSystemId), b.Priorities, b.Approver,
                b.Reason, b.Validity, b.JustificationPrefix, b.EffectivePrefix(b.Reason), Element(b.DiscoveryJson), b.Status, rows.Count,
                ExemptionStatusMath.Totals(rows), ExemptionStatusMath.Quantities(rows), b.CreatedAt, b.CreatedBy);
        })];
    }

    public static async Task<ExemptionBatchResponse> BatchAsync(IDataStore store, ExemptionBatch batch, CancellationToken ct) =>
        (await BatchesAsync(store, [batch], ct))[0];

    public static ExemptionRequestResponse Request(ExemptionRequest r, RemediationItem item) =>
        new(r.Guid, r.Seq, item.N, item.ObjectName, item.ObjectType, item.RefObject, item.Check, item.Message, r.MarkerId, r.Reason, r.Justification, r.Status,
            r.ProbablyNoScope, r.Message, r.RequestXml, r.ResponseXml);

    public static JsonElement? Element(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonDocument.Parse(json).RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
