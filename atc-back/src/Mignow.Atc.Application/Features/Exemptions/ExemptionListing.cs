using System.Text.RegularExpressions;
using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>
/// Monta o lote de findings para o pedido de isenção — porte fiel do <c>listar.py</c> (sem acesso ao SAP). Entrada: itens
/// com status <c>Isenção</c> (ou, com <c>includeObs</c>, sessões antigas cuja observação pede isenção). Vínculo item →
/// finding: <c>Finding ID</c> do item (exato) ou, sem ele, cruzamento com as linhas da extração por objeto + tipo + objeto
/// referenciado + mensagem do check (itens repetidos recebem os findings do grupo, um por item). Findings já isentos na
/// extração e já pedidos com sucesso saem da lista.
/// </summary>
public static partial class ExemptionListing
{
    public static ExemptionListingResult Build(
        IReadOnlyList<ExemptionListingItem> items, IReadOnlyList<ExemptionListingFinding> extraction, IReadOnlySet<string> alreadyRequested,
        IReadOnlyList<string>? priorities, bool includeObs, ISet<int>? onlyItems, ISet<int>? excludeItems)
    {
        var selected = items
            .Where(i => i.Status == RemediationStatus.Exemption || includeObs && ExemptionRules.IsExemptionObservation(i.Obs))
            .Where(i => onlyItems is null || onlyItems.Count == 0 || onlyItems.Contains(i.N))
            .Where(i => excludeItems is null || !excludeItems.Contains(i.N))
            .ToList();

        var byMarker = new Dictionary<string, ExemptionListingFinding>(StringComparer.Ordinal);
        foreach (var f in extraction)
            byMarker[f.MarkerId] = f;
        var index = new Dictionary<(string, string, string, string), List<ExemptionListingFinding>>();
        foreach (var f in extraction)
        {
            var key = (f.ObjectName.ToUpperInvariant(), f.ObjectType.ToUpperInvariant(), f.RefObjectName.ToUpperInvariant(), f.MessageTitle);
            if (!index.TryGetValue(key, out var list))
                index[key] = list = [];
            list.Add(f);
        }

        // Itens com o mesmo vínculo = linhas repetidas da exportação do ATC, uma por finding; os findings do grupo são
        // distribuídos entre os itens (todos recebem o mesmo pedido). Com Finding ID o vínculo é exato.
        var groups = new List<(bool ById, string Id, (string, string, string, string) Key, List<ExemptionListingItem> Items)>();
        var groupIndex = new Dictionary<object, int>();
        foreach (var i in selected)
        {
            object key = !string.IsNullOrEmpty(i.FindingId)
                ? ("id", i.FindingId)
                : ("x", i.ObjectName.ToUpperInvariant(), i.ObjectType.ToUpperInvariant(), (i.RefObject ?? string.Empty).ToUpperInvariant(), i.Message ?? string.Empty);
            if (!groupIndex.TryGetValue(key, out var at))
            {
                groupIndex[key] = at = groups.Count;
                groups.Add(string.IsNullOrEmpty(i.FindingId)
                    ? (false, string.Empty, (i.ObjectName.ToUpperInvariant(), i.ObjectType.ToUpperInvariant(), (i.RefObject ?? string.Empty).ToUpperInvariant(), i.Message ?? string.Empty), [])
                    : (true, i.FindingId, default, []));
            }
            groups[at].Items.Add(i);
        }

        List<ExemptionListingEntryData> entries = [];
        List<ExemptionListingNote> withoutLink = [], exempt = [], already = [], outOfScope = [], withoutPriority = [];
        List<ExemptionDivergentGroup> divergent = [];
        foreach (var (byId, id, key, its) in groups)
        {
            List<ExemptionListingFinding> found;
            string origin;
            if (byId)
            {
                found = [byMarker.GetValueOrDefault(id) ?? Stub(id)];
                origin = "finding_id";
            }
            else
            {
                found = index.GetValueOrDefault(key) ?? [];
                origin = "cruzamento";
            }
            if (found.Count == 0)
            {
                withoutLink.AddRange(its.Select(i => new ExemptionListingNote(i.N, i.ObjectName, null, $"{i.ObjectType} {i.RefObject} {i.Message}".Trim())));
                continue;
            }
            if (origin == "cruzamento" && found.Count != its.Count)
                divergent.Add(new ExemptionDivergentGroup([.. its.Select(i => i.N)], its[0].ObjectName, its[0].RefObject, found.Count));
            for (var j = 0; j < found.Count; j++)
            {
                var f = found[j];
                var item = its[Math.Min(j, its.Count - 1)];
                var priority = !string.IsNullOrEmpty(item.Priority) ? item.Priority : f.Priority;
                if (priorities is not null && string.IsNullOrEmpty(priority))
                {
                    withoutPriority.Add(new ExemptionListingNote(item.N, item.ObjectName, f.MarkerId, null));
                    continue;
                }
                if (priorities is not null && !priorities.Contains(priority!))
                {
                    outOfScope.Add(new ExemptionListingNote(item.N, item.ObjectName, f.MarkerId, priority));
                    continue;
                }
                if (f.ExemptionApproval is not ("-" or ""))
                {
                    exempt.Add(new ExemptionListingNote(item.N, item.ObjectName, f.MarkerId, f.ExemptionApproval));
                    continue;
                }
                if (alreadyRequested.Contains(f.MarkerId))
                {
                    already.Add(new ExemptionListingNote(item.N, item.ObjectName, f.MarkerId, null));
                    continue;
                }
                var (include, line) = Location(f.Location);
                entries.Add(new ExemptionListingEntryData(entries.Count + 1, item, [.. its.Select(i => i.N)], f.MarkerId, origin, include, line, ScopeWarning(item, f)));
            }
        }

        var report = new ExemptionListingReport(
            selected.Count, entries.Count, priorities ?? [], outOfScope.Count, withoutPriority, divergent,
            new Dictionary<string, int>
            {
                ["finding_id"] = entries.Count(e => e.Origin == "finding_id"),
                ["cruzamento"] = entries.Count(e => e.Origin == "cruzamento"),
            },
            withoutLink, exempt, already,
            [.. entries.Where(e => e.ScopeWarning.Length > 0).Select(e => new ExemptionListingNote(e.Item.N, e.Item.ObjectName, e.MarkerId, e.ScopeWarning))],
            [.. entries.Take(8).Select(e => new ExemptionListingEntry(e.Seq, e.Item.N, e.Item.ObjectName, e.Include, e.Line, e.Item.RefObject, e.MarkerId, e.Origin))]);
        return new ExemptionListingResult(entries, report);
    }

    /// <summary>Com Finding ID sem a linha na extração: só o markerId (como <c>{"quickfixInfo": id}</c> do script).</summary>
    private static ExemptionListingFinding Stub(string markerId) => new(markerId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    /// <summary>Include e linha do <c>location</c> do ADT (<c>.../includes/&lt;nome&gt;/source/main#start=974,0</c>).</summary>
    public static (string? Include, int? Line) Location(string? location)
    {
        var m = LocationRegex().Match(location ?? string.Empty);
        return m.Success ? (m.Groups[1].Value.ToUpperInvariant(), int.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)) : (null, null);
    }

    /// <summary>
    /// Motivo provável de o SAP não aceitar isenção só deste finding (<c>thisFinding</c> desativado no modelo). Visto no
    /// EC2/300: finding sem checksum (-1; ex. _SYNERR_) e objeto ABAP Query (AQSG/AQQU). A simulação confirma (SEM_ESCOPO).
    /// </summary>
    public static string ScopeWarning(ExemptionListingItem item, ExemptionListingFinding finding)
    {
        if (finding.Checksum == "-1")
            return "checksum -1";
        var type = (finding.ObjectType.Length > 0 ? finding.ObjectType : item.ObjectType).ToUpperInvariant();
        if (type is "AQSG" or "AQQU" or "AQBG")
            return $"objeto ABAP Query ({type})";
        if (finding.Checksum.Length == 0 && ((item.Message ?? string.Empty).StartsWith("Syntax error", StringComparison.Ordinal)
                                             || (item.Check ?? string.Empty).Contains("SAP Queries", StringComparison.Ordinal)))
            return "syntax error / SAP Query (sem dados da extração)";
        return string.Empty;
    }

    [GeneratedRegex(@"/([^/]+)/source/main.*?#start=(\d+)")]
    private static partial Regex LocationRegex();
}
