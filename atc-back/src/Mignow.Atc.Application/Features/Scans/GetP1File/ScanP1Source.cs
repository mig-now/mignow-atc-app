using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Features.Scans.GetP1File;

/// <summary>Linhas do CSV P1 de uma varredura já cruzada (usado pelo download e pela importação na remediação).</summary>
public sealed record ScanP1Content(string FileName, IReadOnlyList<IReadOnlyList<string>> Rows, byte[] Csv);

public static class ScanP1Source
{
    public static async Task<Result<ScanP1Content>> LoadAsync(IDataStore store, IStringLocalizer<Errors> l, MignowScan scan, bool? includeOtherLine, CancellationToken ct)
    {
        if (scan.SummaryJson is null || JsonNode.Parse(scan.SummaryJson) is not JsonObject summary)
            return l.Conflict("Scan.NotCrossed");
        var csv = summary["csv_remediacao"]!.AsObject();
        var other = includeOtherLine ?? csv["inclui_outra_linha"]!.GetValue<bool>();
        var scanId = scan.Id;
        var occurrences = await store.ListAsync(store.Read<MignowOccurrence>().Where(o => o.ScanId == scanId).OrderBy(o => o.Batch).ThenBy(o => o.Id), ct);
        var selected = ScanP1Csv.Select(occurrences, other);
        var rules = MignowRulesDocument.Parse(scan.RulesSnapshotJson).Rules.ToDictionary(r => r.Id, StringComparer.Ordinal);
        var now = DateTime.ParseExact(summary["gerado_em"]!.GetValue<string>(), "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        var scope = ScanCrossAnalyzer.ScopeLookup(scan.Batches);
        var rows = ScanP1Csv.Rows(selected, scope, rules, now);
        return new ScanP1Content(csv["arquivo"]!.GetValue<string>(), rows, ScanP1Csv.Write(selected, scope, rules, now));
    }
}
