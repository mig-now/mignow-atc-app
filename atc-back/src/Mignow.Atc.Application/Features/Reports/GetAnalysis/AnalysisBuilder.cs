using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Features.Settings;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Domain.Scan;
using static Mignow.Atc.Application.Modules.Report.AnalysisRenderer;

namespace Mignow.Atc.Application.Features.Reports.GetAnalysis;

/// <summary>Monta os três relatórios de análise (<c>analise_isencao|pendencias|mignow.py</c>) a partir do banco.</summary>
internal sealed partial class AnalysisBuilder(IDataStore store, IStringLocalizer<Errors> l, IDateTimeProvider clock, IReportTemplate template, IAiGateway ai,
    IMignowRulesSource rulesSource)
{
    private const int Context = 6;

    public sealed record Built(AnalysisResponse Summary, string Html);

    public async Task<Result<Built>> BuildAsync(Guid customerId, string kindRoute, Guid? sapSystemId, CancellationToken ct)
    {
        var kind = ReportAnalysisKinds.Parse(kindRoute);
        if (kind is null)
            return l.Validation("Report.InvalidAnalysis");
        var customer = await ReportLoader.FindCustomerAsync(store, customerId, ct);
        if (customer is null)
            return l.NotFound("Customer.NotFound");
        var system = await ReportLoader.FindSystemAsync(store, l, customer, sapSystemId, ct);
        if (system.IsFailure)
            return system.Error;
        return kind switch
        {
            ReportAnalysisKind.Exemption => await ExemptionAsync(customer, system.Value, ct),
            ReportAnalysisKind.Pending => await PendingAsync(customer, system.Value, ct),
            _ => await MignowAsync(customer, system.Value, ct),
        };
    }

    // ------------------------------------------------------------ localização do finding e código

    private sealed record Location(string Include, int? Line);

    private async Task<Dictionary<string, Location>> LocationsAsync(int systemId, IEnumerable<string?> findingIds, CancellationToken ct)
    {
        var ids = findingIds.Where(f => !string.IsNullOrEmpty(f)).Select(f => f!).Distinct().ToList();
        var result = new Dictionary<string, Location>(StringComparer.Ordinal);
        if (ids.Count == 0)
            return result;
        var extractionIds = await store.ListAsync(store.Read<AtcExtraction>().Where(e => e.SapSystemId == systemId).OrderBy(e => e.ExtractedAt).Select(e => e.Id), ct);
        var findings = await store.ListAsync(store.Read<AtcFinding>().Where(f => extractionIds.Contains(f.ExtractionId) && ids.Contains(f.FindingId!)).OrderBy(f => f.ExtractionId), ct);
        foreach (var f in findings)
        {
            var columns = ExtractionFiles.ParseRow(f.AllColumnsJson);
            var loc = columns.GetValueOrDefault("location");
            var m = loc is null ? null : LocationRx().Match(loc);
            if (m is { Success: true })
            {
                var parts = m.Groups[1].Value.Split('/');
                var include = Uri.UnescapeDataString(parts[^3]).ToUpperInvariant();
                result[f.FindingId!] = new Location(include, m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : null);
            }
        }
        return result;
    }

    [GeneratedRegex(@"(/sap/bc/adt/\S+?/source/main)(?:\?[^#\s]*)?(?:#start=(\d+))?", RegexOptions.IgnoreCase)]
    private static partial Regex LocationRx();

    /// <summary>Código do include: o backup mais antigo (versão anterior às correções, mesma numeração do ATC) ou o fonte em cache.</summary>
    private sealed class CodeSource(Dictionary<string, (IReadOnlyList<string> Lines, string Origin)> map)
    {
        public (IReadOnlyList<CodeLine>? Lines, string? Origin) Get(string? include, int? line)
        {
            if (string.IsNullOrEmpty(include) || line is null || !map.TryGetValue(include.ToUpperInvariant(), out var s))
                return (null, null);
            return (Window(s.Lines, line.Value, line.Value, Context), s.Origin);
        }
    }

    private async Task<CodeSource> CodeSourceAsync(int systemId, IReadOnlyList<RemediationItem> items, IEnumerable<string?> includes, CancellationToken ct)
    {
        var map = new Dictionary<string, (IReadOnlyList<string>, string)>(StringComparer.Ordinal);
        var backups = items.Where(i => i.Application?.BackupSource is { } b && b.Contains('\n'))
            .OrderBy(i => i.Application!.History.Count > 0 ? i.Application.History[0].At : DateTimeOffset.MaxValue);
        foreach (var i in backups)
        {
            var key = (i.Preparation?.Source ?? i.ObjectName).ToUpperInvariant();
            map.TryAdd(key, (i.Application!.BackupSource!.Split('\n').Select(x => x.TrimEnd('\r')).ToList(), "backup (versão anterior às correções)"));
        }
        var names = includes.Where(x => !string.IsNullOrEmpty(x)).Select(x => x!.ToUpperInvariant()).Distinct().Where(n => !map.ContainsKey(n)).ToList();
        if (names.Count > 0)
        {
            var caches = await store.ListAsync(store.Read<SourceCache>().Where(s => s.SapSystemId == systemId && names.Contains(s.IncludeName.ToUpper())), ct);
            foreach (var c in caches)
                map.TryAdd(c.IncludeName.ToUpperInvariant(), (c.Source.Split('\n').Select(x => x.TrimEnd('\r')).ToList(), "fonte em cache"));
        }
        return new CodeSource(map);
    }

    private async Task<(IReadOnlyList<RemediationItem> Items, RemediationSession? Session)> CurrentSessionAsync(int systemId, CancellationToken ct)
    {
        var sessions = await ReportLoader.LoadSessionsAsync(store, systemId, ct);
        var last = sessions.LastOrDefault();
        return last is null ? ([], null) : (last.Items, last.Session);
    }

    // ------------------------------------------------------------ isenção

    private async Task<Result<Built>> ExemptionAsync(Customer customer, SapSystem system, CancellationToken ct)
    {
        var batches = await store.ListAsync(store.Read<ExemptionBatch>().Where(b => b.SapSystemId == system.Id).OrderBy(b => b.Id), ct);
        var batch = batches.LastOrDefault();
        if (batch is null)
            return l.NotFound("Report.NoExemption");
        var requests = await store.ListAsync(store.Read<ExemptionRequest>().Where(r => r.BatchId == batch.Id).OrderBy(r => r.Seq), ct);
        var (items, _) = await CurrentSessionAsync(system.Id, ct);
        var byId = items.ToDictionary(i => i.Id);
        var locations = await LocationsAsync(system.Id, requests.Select(r => r.MarkerId).Concat(items.Select(i => i.FindingId)), ct);
        var code = await CodeSourceAsync(system.Id, items, locations.Values.Select(x => x.Include), ct);
        var sent = requests.Any(r => r.Status is not (null or ExemptionRequestStatus.Simulated));
        var extraction = await store.FirstOrDefaultAsync(store.Read<AtcExtraction>().Where(e => e.SapSystemId == system.Id).OrderByDescending(e => e.ExtractedAt), ct);
        var rows = new List<ExemptionRow>();
        foreach (var r in requests.Where(r => byId.ContainsKey(r.ItemId)))
        {
            var it = byId[r.ItemId];
            locations.TryGetValue(r.MarkerId, out var loc);
            var noCode = AnalysisRenderer.NoCodeTypes.Contains(it.ObjectType.ToUpperInvariant());
            var (lines, origin) = noCode ? (null, null) : code.Get(loc?.Include, loc?.Line);
            rows.Add(new ExemptionRow(r.Seq, it.N, it.ObjectName, it.ObjectType, loc?.Include, loc?.Line, it.RefObject, it.Check ?? "", it.Message ?? "", r.Reason, r.Status,
                r.Justification, it.Obs, it.Priority ?? "1", it.RefType, it.Note, "", lines, origin));
        }
        var input = new ExemptionInput(customer.Name, system.Sid, system.Client, system.ExemptionApprover ?? batch.Approver, extraction?.Series,
            extraction?.ExecutedAtUtc?.UtcDateTime.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture), $"lote_{batch.Id}", sent ? "envio" : "simulação", rows, clock.UtcNow);
        var html = RenderExemption(input, template.Css);
        var groups = rows.Select(x => $"{ExemptionRuleOf(x.Obs)}|{x.Ref}|{x.Check}").Distinct().Count();
        var byReason = rows.GroupBy(x => x.Reason ?? "–").ToDictionary(g => g.Key, g => g.Count());
        var byResult = rows.GroupBy(x => x.Result ?? "–").ToDictionary(g => g.Key, g => g.Count());
        return new Built(new AnalysisResponse("isencao", $"Isenções do {customer.Name}", rows.Count, rows.Select(x => (x.Type, x.Obj)).Distinct().Count(), groups,
            rows.Count(x => x.Snippet is { Count: > 0 }), byReason, byResult), html);
    }

    // ------------------------------------------------------------ pendências

    private async Task<Result<Built>> PendingAsync(Customer customer, SapSystem system, CancellationToken ct)
    {
        var (items, session) = await CurrentSessionAsync(system.Id, ct);
        if (session is null)
            return l.NotFound("Report.NoSession");
        var selected = items.Where(i => i.Status == RemediationStatus.Pending || (i.Status.StartsWith("Confirmado", StringComparison.Ordinal) && i.Obs.Contains("MANUAL", StringComparison.Ordinal))).ToList();
        var locations = await LocationsAsync(system.Id, selected.Select(i => i.FindingId), ct);
        var code = await CodeSourceAsync(system.Id, items, locations.Values.Select(x => x.Include), ct);
        var rows = new List<PendingRow>();
        foreach (var i in selected)
        {
            var group = i.Status == RemediationStatus.Pending ? "SD" : ManualGroup(i.Obs);
            var reason = i.Status == RemediationStatus.Pending
                ? (string.IsNullOrWhiteSpace(i.Obs) ? "Sem decisão registrada na sessão." : Regex.Replace(i.Obs, @"\s+", " "))
                : Regex.Replace(i.Obs, @"\s+", " ");
            Location? loc = null;
            if (i.FindingId is not null)
                locations.TryGetValue(i.FindingId, out loc);
            var (lines, origin) = AnalysisRenderer.NoCodeTypes.Contains(i.ObjectType.ToUpperInvariant()) ? (null, null) : code.Get(loc?.Include, loc?.Line);
            rows.Add(new PendingRow(group, PendingTitles[group], i.N, i.ObjectName, i.ObjectType, i.Package, i.Priority, i.RefObject, i.RefType, i.Note, i.Check ?? "",
                i.Message ?? "", i.Status, reason, loc?.Include, loc?.Line, lines, origin, string.IsNullOrEmpty(i.Proposal?.MarkedBlock) ? [] : [i.Proposal!.MarkedBlock!], i.N));
        }
        var html = RenderPending(new PendingInput(customer.Name, system.Sid, system.Client, session.ProjectType.ToString(), session.Request, rows, clock.UtcNow), template.Css);
        var byGroup = rows.GroupBy(r => r.Group).ToDictionary(g => g.Key, g => g.Count());
        var extra = new Dictionary<string, int>
        {
            ["pendentes"] = rows.Count(r => r.Group == "SD"), ["manuais"] = rows.Count(r => r.Group != "SD"), ["comBloco"] = rows.Count(r => r.Blocks.Count > 0),
        };
        return new Built(new AnalysisResponse("pendencias", $"Pendências do {customer.Name}", rows.Count, rows.Select(r => (r.Type, r.Obj)).Distinct().Count(), byGroup.Count,
            rows.Count(r => r.Snippet is { Count: > 0 }), byGroup, extra), html);
    }

    // ------------------------------------------------------------ MIGNOW

    private async Task<Result<Built>> MignowAsync(Customer customer, SapSystem system, CancellationToken ct)
    {
        var scan = await store.FirstOrDefaultAsync(store.Read<MignowScan>().Where(s => s.SapSystemId == system.Id).OrderByDescending(s => s.Id), ct);
        if (scan is null)
            return l.NotFound("Report.NoScan");
        var occurrences = await store.ListAsync(store.Read<MignowOccurrence>().Where(o => o.ScanId == scan.Id).OrderBy(o => o.Id), ct);
        var settings = await SettingsStore.LoadAsync(store, false, ct);
        var rules = await rulesSource.LoadAsync(settings.MignowRulesPath, ct);
        var pending = occurrences.Where(o => o.Cross is not null && o.Cross != ScanCross.AtcCaught
            && (string.IsNullOrEmpty(o.Class) || string.Equals(o.Class, "pendente", StringComparison.OrdinalIgnoreCase))).ToList();
        var uris = pending.Select(o => o.AdtSource).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToList();
        var caches = uris.Count == 0 ? [] : await store.ListAsync(store.Read<SourceCache>().Where(s => s.SapSystemId == system.Id && uris.Contains(s.AdtUri)), ct);
        var sources = caches.GroupBy(c => c.AdtUri).ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.First().Source.Split('\n').Select(x => x.TrimEnd('\r')).ToList());
        var rows = new List<MignowRow>();
        var seq = 0;
        foreach (var o in pending.OrderBy(o => o.Id))
        {
            seq++;
            rules.TryGetValue(o.RuleId, out var rule);
            var end = o.LineEnd ?? o.Line;
            var element = !string.IsNullOrEmpty(rule?.FixedElement) ? rule!.FixedElement! : ElementOf(rule?.RefinedPcre, o.Snippet);
            sources.TryGetValue(o.AdtSource ?? "", out var lines);
            rows.Add(new MignowRow(seq, o.RuleId, o.ObjectType, o.ObjectName, null, null, o.AdtSource ?? "", o.Line, end, element, o.Snippet ?? "", o.Cross!, rule?.Category,
                rule?.Severity, rule?.Description, rule?.RefinedPcre, Window(lines, o.Line, end, Context)));
        }
        var roles = await RoleTextsAsync(rows, ct);
        var summary = SummaryOf(scan, system, occurrences, pending, rows);
        var input = new MignowInput(customer.Name, null, $"varredura_{scan.Id}", summary, rows, clock.UtcNow, roles);
        var html = RenderMignow(input, template.Css);
        var byCross = rows.GroupBy(r => r.Cross).ToDictionary(g => g.Key, g => g.Count());
        var byRule = rows.GroupBy(r => r.Rule).ToDictionary(g => g.Key, g => g.Count());
        return new Built(new AnalysisResponse("mignow", $"O que o ATC não pegou no {customer.Name}", rows.Count, rows.Select(r => (r.ObjType, r.ObjName)).Distinct().Count(),
            byRule.Count, rows.Count(r => r.Context is { Count: > 0 }), byRule, byCross), html);
    }

    private static string ElementOf(string? pcre, string? snippet)
    {
        if (string.IsNullOrEmpty(pcre) || string.IsNullOrEmpty(snippet))
            return "–";
        try
        {
            var m = Regex.Match(snippet, pcre, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
            return m.Success && m.Groups.Count > 1 && m.Groups[1].Success ? m.Groups[1].Value.ToUpperInvariant() : "–";
        }
        catch (Exception e) when (e is ArgumentException or RegexMatchTimeoutException)
        {
            return "–";
        }
    }

    private static MignowSummary SummaryOf(MignowScan scan, SapSystem system, IReadOnlyList<MignowOccurrence> all, IReadOnlyList<MignowOccurrence> pending, IReadOnlyList<MignowRow> rows)
    {
        int Int(JsonElement e, params string[] path)
        {
            foreach (var p in path)
            {
                if (e.ValueKind != JsonValueKind.Object || !e.TryGetProperty(p, out e))
                    return 0;
            }
            return e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var v) ? v : 0;
        }
        string? Str(JsonElement e, params string[] path)
        {
            foreach (var p in path)
            {
                if (e.ValueKind != JsonValueKind.Object || !e.TryGetProperty(p, out e))
                    return null;
            }
            return e.ValueKind == JsonValueKind.String ? e.GetString() : null;
        }
        var coverage = new List<MignowCoverage>();
        JsonElement root = default;
        var hasSummary = false;
        if (!string.IsNullOrWhiteSpace(scan.SummaryJson))
        {
            try
            {
                root = JsonDocument.Parse(scan.SummaryJson).RootElement.Clone();
                hasSummary = root.ValueKind == JsonValueKind.Object;
            }
            catch (JsonException)
            {
            }
        }
        if (hasSummary && root.TryGetProperty("cobertura_atc_por_elemento", out var cov) && cov.ValueKind == JsonValueKind.Object)
        {
            foreach (var c in cov.EnumerateObject())
                coverage.Add(new MignowCoverage(c.Name, Int(c.Value, "pendentes"), Int(c.Value, "atc_pegou"), Int(c.Value, "mignow"),
                    c.Value.TryGetProperty("cobertura_atc_pct", out var pct) && pct.ValueKind == JsonValueKind.Number ? pct.GetDouble() : 0));
        }
        if (!hasSummary)
        {
            return new MignowSummary(system.DisplayName, null, null, 0, 0, 0, 0, all.Count, all.Count(o => string.Equals(o.Class, "I", StringComparison.OrdinalIgnoreCase)),
                all.Count(o => string.Equals(o.Class, "R", StringComparison.OrdinalIgnoreCase)), pending.Count, all.Count(o => o.Cross == ScanCross.AtcCaught),
                all.Count(o => o.Cross == ScanCross.AtcOtherLine), coverage);
        }
        return new MignowSummary(Str(root, "sistema") ?? system.DisplayName, Str(root, "base_atc", "serie"), (Str(root, "base_atc", "executada_utc") ?? "").PadRight(8)[..8].Trim(),
            Int(root, "base_atc", "findings"), Int(root, "escopo", "objetos"), Int(root, "lidos", "fontes"), Int(root, "lidos", "linhas"), Int(root, "ocorrencias", "total"),
            Int(root, "ocorrencias", "informativo"), Int(root, "ocorrencias", "ja_remediado"), Int(root, "ocorrencias", "pendente"),
            Int(root, "cruzamento_pendentes", "ATC_PEGOU"), Int(root, "cruzamento_pendentes", "ATC_OUTRA_LINHA"), coverage);
    }

    /// <summary>"O que cada papel analisa" por categoria: recomendação da IA (Review); falha da IA = texto fixo do script.</summary>
    private async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> RoleTextsAsync(IReadOnlyList<MignowRow> rows, CancellationToken ct)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, string>>();
        var schema = JsonDocument.Parse("""
            {"type":"object","additionalProperties":false,"required":["consultorAbap","funcional","cliente"],
             "properties":{"consultorAbap":{"type":"string"},"funcional":{"type":"string"},"cliente":{"type":"string"}}}
            """).RootElement.Clone();
        foreach (var g in rows.Where(r => r.Category is not null && MignowRoleTexts.ByCategory.ContainsKey(r.Category)).GroupBy(r => r.Category!))
        {
            var user = $"Categoria: {g.Key}\nRegras e descrições:\n" + string.Join("\n", g.Select(r => $"- {r.Rule}: {r.Description}").Distinct().Take(8));
            var request = new AiRequest(AiUsage.Review, MignowRoleTexts.Purpose,
                "Você apoia consultores SAP em adaptação de código custom ao S/4HANA. Para a categoria de regra de varredura informada, escreva em português do Brasil, em uma ou duas frases objetivas cada, "
                + "o que o consultor ABAP, o funcional e o cliente devem analisar. Use só o que está nas descrições das regras; não invente comportamento do sistema.",
                user, schema);
            // Geração do relatório não é uma execução (Run): o uso de IA não é atribuído a um run.
            var answer = await ai.CompleteAsync<MignowRolesAnswer>(request, _ => { }, ct);
            if (answer.IsSuccess && !string.IsNullOrWhiteSpace(answer.Value.ConsultorAbap) && !string.IsNullOrWhiteSpace(answer.Value.Funcional) && !string.IsNullOrWhiteSpace(answer.Value.Cliente))
                result[g.Key] = new Dictionary<string, string>
                {
                    [MignowRoleTexts.Abap] = answer.Value.ConsultorAbap, [MignowRoleTexts.Functional] = answer.Value.Funcional, [MignowRoleTexts.Customer] = answer.Value.Cliente,
                };
        }
        return result;
    }
}
