using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions;

/// <summary>Carregamento e montagem das respostas de sessão e item.</summary>
public static class SessionViews
{
    public static Task<RemediationSession?> LoadAsync(IDataStore store, Guid id, bool tracked, CancellationToken ct) =>
        store.FirstOrDefaultAsync((tracked ? store.Query<RemediationSession>() : store.Read<RemediationSession>()).Where(s => s.Guid == id), ct);

    public static Task<SapSystem?> LoadSystemAsync(IDataStore store, int id, CancellationToken ct) =>
        store.FirstOrDefaultAsync(store.Including(store.Read<SapSystem>(), nameof(SapSystem.Customer)).Where(s => s.Id == id), ct);

    public static async Task<IReadOnlyList<SessionResponse>> BuildAsync(IDataStore store, IReadOnlyList<RemediationSession> sessions, CancellationToken ct,
        ImportSummary? import = null, int carried = 0)
    {
        if (sessions.Count == 0)
            return [];
        var ids = sessions.Select(s => s.Id).ToArray();
        var systemIds = sessions.Select(s => s.SapSystemId).Distinct().ToArray();
        var systems = (await store.ListAsync(store.Including(store.Read<SapSystem>(), nameof(SapSystem.Customer)).Where(s => systemIds.Contains(s.Id)), ct))
            .ToDictionary(s => s.Id);
        var rows = await store.ListAsync(
            store.Read<RemediationItem>().Where(i => ids.Contains(i.SessionId)).Select(i => new { i.SessionId, i.Status, i.ObjectType, i.ObjectName, i.ApplicationResult, i.AwaitingApproval }), ct);
        return [.. sessions.Select(s =>
        {
            var system = systems[s.SapSystemId];
            var mine = rows.Where(r => r.SessionId == s.Id).ToList();
            // Sessão recém-criada: os itens ainda não foram gravados (o commit é do decorator), então conta os da navegação.
            if (mine.Count == 0 && s.Items.Count > 0)
                mine = [.. s.Items.Select(i => new { i.SessionId, i.Status, i.ObjectType, i.ObjectName, i.ApplicationResult, i.AwaitingApproval })];
            int N(string status) => mine.Count(r => r.Status == status);
            var counts = new SituationCounts(N(RemediationStatus.ConfirmedKb), N(RemediationStatus.ConfirmedAi), N(RemediationStatus.Exemption),
                N(RemediationStatus.Cancelled), N(RemediationStatus.Pending), mine.Select(r => (r.ObjectType, r.ObjectName)).Distinct().Count(),
                mine.Where(r => r.Status.Length == 0).Select(r => (r.ObjectType, r.ObjectName)).Distinct().Count(),
                mine.Count(r => ApplyResults.IsFailure(r.ApplicationResult)),
                mine.Count(r => r.Status == RemediationStatus.Pending && r.AwaitingApproval));
            return new SessionResponse(s.Guid, system.Customer.Guid, system.Customer.Name, system.Guid, system.DisplayName, s.Source, s.SourceRef, s.Priorities,
                s.Request, s.Author, s.ProjectType, s.Cycle, s.Archived, s.ArchivedAt, s.AutoMode, s.CsvEncoding, s.CsvDelimiter,
                Json.Read(s.CsvColumnsJson, new List<string>()), s.CreatedAt, counts, import, carried);
        })];
    }

    public static ItemResponse ToResponse(this RemediationItem i) =>
        new(i.Guid, i.N, i.ObjectName, i.ObjectType, i.RefObject, i.RefType, i.Check, i.Message, i.Note, i.Text, i.Package, i.Classification, i.Priority,
            i.FindingId, i.Origin, i.Status, i.Obs, i.StatusDate, i.TriageFlags, i.Proposal is not null, i.Preparation is not null, i.ApplicationResult,
            i.Application?.History.LastOrDefault()?.Detail is { Length: > 0 } d ? d : null,
            i.Status == RemediationStatus.Pending && i.AwaitingApproval,
            i.Status == RemediationStatus.Pending && i.AwaitingApproval ? i.Proposal?.Justification : null,
            i.Status == RemediationStatus.Pending && i.AwaitingApproval ? i.Proposal?.Action : null);

    public static ItemDetailResponse ToDetail(this RemediationItem i) =>
        new(i.ToResponse(), i.StatusHistory, i.Preparation, i.Proposal, i.Application, i.CorrectionSheetMd, Json.Read(i.RawRowJson, new List<string>()));

    /// <summary>Fontes lidos do ambiente (cache) dos objetos pedidos, como índice de consulta.</summary>
    public static async Task<SourceIndex> SourcesAsync(IDataStore store, int sapSystemId, IReadOnlyCollection<string> objects, CancellationToken ct)
    {
        var names = objects.Select(o => o.ToUpperInvariant()).Distinct().ToArray();
        var rows = await store.ListAsync(
            store.Read<SourceCache>().Where(c => c.SapSystemId == sapSystemId && names.Contains(c.ObjectName)).Select(c => new { c.ObjectName, c.IncludeName, c.Source }), ct);
        return new SourceIndex(rows.Select(r => new SourceFileText(r.ObjectName, r.IncludeName, r.Source)));
    }
}
