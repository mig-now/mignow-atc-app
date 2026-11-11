using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Dtos.Scans;

/// <summary>Monta as respostas da varredura (nomes do cadastro e ids públicos da extração).</summary>
public static class ScanViews
{
    public static Task<MignowScan?> LoadAsync(IDataStore store, Guid id, bool tracked, CancellationToken ct) =>
        store.FirstOrDefaultAsync((tracked ? store.Query<MignowScan>() : store.Read<MignowScan>()).Where(s => s.Guid == id), ct);

    public static async Task<IReadOnlyList<ScanResponse>> BuildAsync(IDataStore store, IReadOnlyList<MignowScan> scans, CancellationToken ct)
    {
        if (scans.Count == 0)
            return [];
        var systemIds = scans.Select(s => s.SapSystemId).Distinct().ToArray();
        var systems = (await store.ListAsync(store.Read<SapSystem>().Where(s => systemIds.Contains(s.Id)), ct)).ToDictionary(s => s.Id);
        var extractionIds = scans.Where(s => s.ExtractionId is not null).Select(s => s.ExtractionId!.Value).Distinct().ToArray();
        var extractions = (await store.ListAsync(store.Read<AtcExtraction>().Where(e => extractionIds.Contains(e.Id)).Select(e => new { e.Id, e.Guid }), ct))
            .ToDictionary(e => e.Id, e => e.Guid);
        return [.. scans.Select(s =>
        {
            var system = systems[s.SapSystemId];
            var rules = MignowRulesDocument.Parse(s.RulesSnapshotJson);
            return new ScanResponse(s.Guid, system.Guid, system.DisplayName, s.ExtractionId is { } x && extractions.TryGetValue(x, out var eg) ? eg : null,
                RunMappings.Element(s.ScopeCountsJson),
                [.. s.Batches.Select(b => new ScanBatchResponse(b.Number, b.Weight, b.Objects.Count, b.ClassName, b.Status, b.ClassRemoved, b.Stats))],
                [.. rules.ActiveRules.Select(r => r.Id)], rules.Rules.Count, RunMappings.Element(s.SummaryJson), s.CreatedAt, s.CreatedBy);
        })];
    }

    public static async Task<ScanResponse> BuildAsync(IDataStore store, MignowScan scan, CancellationToken ct) => (await BuildAsync(store, [scan], ct))[0];

    public static ScanOccurrenceResponse ToResponse(this MignowOccurrence o) =>
        new(o.Guid, o.RuleId, o.ObjectType, o.ObjectName, o.Include, o.AdtSource, o.Line, o.LineEnd, o.Element, o.Snippet, ScanCrossAnalyzer.Situation(o.Class),
            o.Cross, o.Batch);

    public static ScanRulesResponse ToResponse(this MignowRulesDocument d) =>
        new(d.Version, d.Date, d.Trigger, [.. d.Rules.Select(r => new ScanRuleResponse(r.Id, r.Active, r.Priority, r.Category, r.Severity, r.Description,
            r.OriginalPcre, r.Pcre, r.Text, r.FixedElement, r.Requires, r.SuppressedBy, r.Robt))]);
}
