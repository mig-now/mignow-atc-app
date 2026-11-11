using System.Text.Json;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Exemptions.Create;

/// <summary>
/// Cria o lote (porte do <c>listar.py</c>): vincula cada item ao finding exato pelo Finding ID (sessões antigas, por
/// cruzamento com os findings novos — sem baseline — da extração mais recente do ambiente), tira o que já está isento ou
/// já foi pedido e cria um pedido (<see cref="ExemptionRequest"/>) por finding.
/// </summary>
public sealed class CreateExemptionBatchHandler(IDataStore store, IStringLocalizer<Errors> l)
    : ICommandHandler<CreateExemptionBatchCommand, Result<ExemptionBatchCreatedResponse>>
{
    public async Task<Result<ExemptionBatchCreatedResponse>> Handle(CreateExemptionBatchCommand command, CancellationToken ct)
    {
        var session = await store.FirstOrDefaultAsync(store.Read<RemediationSession>().Where(s => s.Guid == command.SessionId), ct);
        if (session is null)
            return l.NotFound("Exemption.SessionNotFound");
        var sessionId = session.Id;
        var systemId = session.SapSystemId;
        var system = await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(s => s.Id == systemId), ct);
        var priorities = ExemptionRules.ParsePriorities(command.Priorities)!;

        var items = (await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == sessionId).OrderBy(i => i.N), ct))
            .Select(i => new ExemptionListingItem(i.Id, i.N, i.ObjectName, i.ObjectType, i.RefObject, i.Check, i.Message, i.Obs, i.Status, i.Priority, i.FindingId))
            .ToList();
        bool IsSelected(ExemptionListingItem i) => i.Status == RemediationStatus.Exemption || command.IncludeObs && ExemptionRules.IsExemptionObservation(i.Obs);
        var candidates = items.Where(IsSelected).ToList();
        if (candidates.Count == 0)
            return l.Validation("Exemption.NoItems");

        // O CSV da extração só é obrigatório para o cruzamento; com Finding ID ele completa checksum/local (aviso de escopo).
        var extraction = await LatestFindingsAsync(systemId, ct);
        if (extraction is null && candidates.Any(i => string.IsNullOrEmpty(i.FindingId)))
            return l.Validation("Exemption.ExtractionRequired");

        var already = (await store.ListAsync(store.Read<ExemptionRequest>()
            .Where(r => r.Status == ExemptionRequestStatus.Requested && store.Read<ExemptionBatch>().Any(b => b.Id == r.BatchId && b.SapSystemId == systemId))
            .Select(r => r.MarkerId), ct)).ToHashSet(StringComparer.Ordinal);

        var listing = ExemptionListing.Build(items, extraction ?? [], already, priorities, command.IncludeObs,
            command.Items is null ? null : new HashSet<int>(command.Items), command.Exclude is null ? null : new HashSet<int>(command.Exclude));
        if (listing.Entries.Count == 0)
            return Error.Validation("Exemption.NoFindings", l.Text("Exemption.NoFindings")) with { Data = listing.Report };

        var batch = ExemptionBatch.Create(sessionId, systemId, string.Join(',', priorities), system?.ExemptionApprover, null);
        foreach (var e in listing.Entries)
            batch.AddRequest(e.Item.Id, e.MarkerId, e.Seq, e.ScopeWarning.Length > 0);
        store.Add(batch);
        // O Id público existe desde a criação; a resposta sai sem consultar o banco (o commit é do decorator).
        var empty = new Dictionary<string, int> { [ExemptionStatusMath.PendingKey] = listing.Entries.Count };
        var response = new ExemptionBatchResponse(batch.Guid, session.Guid, system?.Guid ?? Guid.Empty, batch.Priorities, batch.Approver, batch.Reason,
            batch.Validity, batch.JustificationPrefix, batch.EffectivePrefix(batch.Reason), null, batch.Status, listing.Entries.Count, empty,
            ExemptionStatusMath.Quantities([.. listing.Entries.Select(_ => ((string?)null, false))]), default, string.Empty);
        return new ExemptionBatchCreatedResponse(response, listing.Report);
    }

    /// <summary>Findings novos (sem baseline) da extração mais recente do ambiente; nulo quando não há extração.</summary>
    private async Task<List<ExemptionListingFinding>?> LatestFindingsAsync(int systemId, CancellationToken ct)
    {
        var latest = await store.FirstOrDefaultAsync(store.Read<AtcExtraction>().Where(e => e.SapSystemId == systemId)
            .OrderByDescending(e => e.ExecutedAtUtc).ThenByDescending(e => e.ExtractedAt), ct);
        if (latest is null)
            return null;
        var extractionId = latest.Id;
        var rows = await store.ListAsync(store.Read<AtcFinding>().Where(f => f.ExtractionId == extractionId && !f.InBaseline && f.FindingId != null)
            .OrderBy(f => f.Seq), ct);
        return [.. rows.Select(f =>
        {
            var columns = ExtractionFiles.ParseRow(f.AllColumnsJson);
            return new ExemptionListingFinding(f.FindingId!, f.ObjectName, f.ObjectType ?? string.Empty, f.RefObjectName ?? string.Empty,
                f.MessageTitle ?? string.Empty, f.Priority?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                columns.GetValueOrDefault("checksum", string.Empty), columns.GetValueOrDefault("location", string.Empty),
                columns.GetValueOrDefault("exemptionApproval", string.Empty));
        })];
    }
}
