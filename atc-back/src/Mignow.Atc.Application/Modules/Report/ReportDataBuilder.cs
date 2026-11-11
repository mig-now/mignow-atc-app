using System.Globalization;
using System.Text.RegularExpressions;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>
/// Porte do <c>coletar.py</c> (skill atc-resultado): monta o <see cref="ReportData"/> (= <c>dados.json</c>) a partir do banco.
/// Mesmas chaves e mesmos cálculos: ciclos, burndown por findings e por objetos, check × solução, manual × IA, isenção, KB,
/// execução do processo, custos e avisos. Pura (sem acesso a banco): o <see cref="ReportInput"/> traz o que foi lido.
/// <para>
/// Diferenças dos arquivos para o banco (registradas no relatório do módulo): a evolução (burndown) é derivada do
/// histórico de status dos itens, em lotes separados por pausas &gt; 10 min, no lugar do <c>historico.json</c> por conversa;
/// custos e execução vêm dos <c>Run</c> (tokens de entrada/saída, sem cache) via <see cref="ProcessHistoryBuilder"/>.
/// </para>
/// </summary>
public static partial class ReportDataBuilder
{
    private const string DefaultPremiseSource = "Parâmetros gerais (ManualObjectsPerDay e HoursPerDay)";
    private static readonly string[] Priorities = ["p1", "p2", "p3"];

    public static string Solution(string? status)
    {
        var s = status ?? "";
        return s.Contains("KB", StringComparison.Ordinal) ? "KB"
            : s.Contains("IA", StringComparison.Ordinal) ? "Análise"
            : s.StartsWith("Cancel", StringComparison.Ordinal) ? "Cancelado"
            : s.StartsWith("Isen", StringComparison.Ordinal) ? "Isenção"
            : "Pendente";
    }

    private sealed class Cycle
    {
        public int N;
        public SessionData Data = null!;
        public string Source = "";
        public DateTimeOffset? Backup;
        public DateTimeOffset? Start;
        public ReportData.ExtractionInfo? Extraction;
        public List<string> Scope = [];
    }

    private sealed class Row
    {
        public int N;
        public int Cycle;
        public string Obj = "", Type = "", Check = "", Msg = "", Ref = "", Sol = "", Sis = "", Req = "", Obs = "", Date = "";
        public List<ReportData.HistoryEntry> Hist = [];
        public RemediationItem Item = null!;
        public string Key => $"{Type} {Obj}";
    }

    private sealed class Phase
    {
        public string Stage = "";
        public int Cycle;
        public DateTimeOffset Start, End;
        public int Sessions;
        public long Net, Total, Output, InputWrite;
        public long CacheRead = 0;
    }

    private sealed class ExemptionCycleCalc
    {
        public int Cycle;
        public string Batch = "";
        public int Findings, Requested, NoScope, Blocked, Errors, AlreadyRequested, SimulatedNotSent;
        public int CheckedOk = 0;
        public Dictionary<string, int> Causes = [];
        public List<string> ToCheck = [];
        public List<string> Reasons = [];
        public DateTimeOffset? Date;
    }

    public static ReportData Build(ReportInput input)
    {
        if (input.Sessions.Count == 0)
            throw new ArgumentException("O relatório precisa de ao menos uma sessão de remediação.", nameof(input));
        var zone = input.Zone;
        string Local(DateTimeOffset t) => TimeZoneInfo.ConvertTime(t, zone).ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
        string Rot(DateTimeOffset t) => TimeZoneInfo.ConvertTime(t, zone).ToString("dd.MM HH:mm", CultureInfo.InvariantCulture);
        string Iso(DateTimeOffset t) => t.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        var warnings = new List<string>();
        var system = input.System;
        var highlights = input.Highlights;

        // ---- ciclos: sessões em ordem de ciclo; as arquivadas são os ciclos anteriores
        var ordered = input.Sessions.OrderBy(s => s.Session.Cycle).ThenBy(s => s.CreatedAt).ToList();
        var cycles = new List<Cycle>();
        for (var k = 0; k < ordered.Count; k++)
        {
            var s = ordered[k];
            DateTimeOffset? backup = null;
            if (k < ordered.Count - 1)
                backup = s.Session.ArchivedAt ?? ordered[k + 1].CreatedAt;
            cycles.Add(new Cycle { N = k + 1, Data = s, Source = $"sessions/{s.Session.Guid}", Backup = backup });
        }
        var extracted = input.Extractions.Where(x => x.Situation == ExtractionSituation.Extracted).OrderBy(x => x.ExtractedAt).ToList();
        for (var k = 1; k < cycles.Count; k++)
        {
            var ant = cycles[k - 1];
            var ex = extracted.Select(x => x.ExtractedAt)
                .Where(t => t <= ant.Backup && (ant.Start is null || t > ant.Start)).ToList();
            cycles[k].Start = ex.Count > (ant.Start is not null ? 0 : 1) ? ex.Max() : ant.Backup;
        }
        int CycleOf(DateTimeOffset t) => cycles.Where(c => c.Start is null || t >= c.Start).Max(c => c.N);

        ReportData.ExtractionInfo ExtInfo(AtcExtraction h) => new(h.Series, h.DisplayId, h.ExecutedAtUtc is null ? null : Local(h.ExecutedAtUtc.Value),
            h.TotalCount, h.InBaselineCount, h.TotalCount - h.InBaselineCount, h.P1New, h.P2New, h.P3New, "extractions");
        foreach (var c in cycles)
        {
            var h = extracted.LastOrDefault(x => CycleOf(x.ExtractedAt) == c.N);
            c.Extraction = h is null ? null : ExtInfo(h);
            var pr = c.Data.Session.Priorities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => "p" + p.ToLowerInvariant().TrimStart('p')).ToList();
            c.Scope = [.. Priorities.Where(pr.Contains)];
            if (c.Scope.Count == 0)
                c.Scope = [.. Priorities.Where(p => c.Extraction is not null && PriorityOf(c.Extraction, p) == c.Data.Items.Count)];
        }
        var extraction = cycles.Count > 0 && cycles[0].Extraction is not null ? cycles[0].Extraction
            : extracted.Count > 0 ? ExtInfo(extracted[^1]) : null;
        if (extraction is null)
            warnings.Add("Sem extração registrada (extracoes/historico.json): seção 'Ponto de partida' sem os totais do ATC");
        var scope = Priorities.Where(p => cycles.Any(c => c.Scope.Contains(p))).ToList();

        // ---- isenção: em cada ciclo, o lote mais recente com envio ou simulação; somados no relatório
        var batches = input.Batches.Where(b => b.Requests.Any(r => r.Request.Status is not null)).OrderBy(b => b.At).ToList();
        var exemptionStatus = new Dictionary<(int Cycle, int N), string>();
        var exemptionCycles = new List<ExemptionCycleCalc>();
        foreach (var c in cycles)
        {
            var inCycle = batches.Where(b => CycleOf(b.At) == c.N).ToList();
            if (inCycle.Count == 0)
                continue;
            var batch = inCycle[^1];
            var calc = new ExemptionCycleCalc { Cycle = c.N, Batch = batch.Name, Findings = batch.Requests.Count };
            foreach (var r in batch.Requests)
            {
                var st = r.Request.Status ?? "";
                exemptionStatus[(c.N, r.ItemN)] = st;
                if (st == ExemptionRequestStatus.NoScope)
                {
                    var m = r.Request.Message ?? "";
                    var cause = m.Contains("checksum", StringComparison.Ordinal) ? "Syntax error (checksum -1)"
                        : m.Contains("Query", StringComparison.Ordinal) ? "ABAP Query" : (m.Length > 80 ? m[..80] : m);
                    calc.Causes[cause] = calc.Causes.GetValueOrDefault(cause) + 1;
                }
            }
            calc.Reasons = [.. batch.Requests.Where(r => r.Request.Status == ExemptionRequestStatus.Requested).Select(r => r.Request.Reason)
                .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
            var q = batch.Requests.GroupBy(r => r.Request.Status ?? "").ToDictionary(g => g.Key, g => g.Count());
            calc.Requested = q.GetValueOrDefault(ExemptionRequestStatus.Requested);
            calc.NoScope = q.GetValueOrDefault(ExemptionRequestStatus.NoScope);
            calc.Blocked = q.GetValueOrDefault(ExemptionRequestStatus.Blocked);
            calc.Errors = q.Where(kv => kv.Key.StartsWith("ERRO", StringComparison.Ordinal)).Sum(kv => kv.Value);
            calc.AlreadyRequested = q.GetValueOrDefault(ExemptionRequestStatus.AlreadyRequested);
            calc.SimulatedNotSent = q.GetValueOrDefault(ExemptionRequestStatus.Simulated);
            var sends = input.Runs.Where(r => r.Run.Kind == RunKind.ExemptionSend && r.Run.SubjectId == batch.Batch.Guid && r.Run.FinishedAt is not null)
                .Select(r => r.Run.FinishedAt!.Value).ToList();
            calc.Date = sends.Count > 0 ? sends.Max() : null;
            exemptionCycles.Add(calc);
        }
        ReportData.ExemptionInfo? exemption = null;
        if (exemptionCycles.Count > 0)
        {
            var date = exemptionCycles.Where(x => x.Date is not null).Select(x => x.Date!.Value).DefaultIfEmpty().Max();
            exemption = new ReportData.ExemptionInfo(
                string.Join(", ", exemptionCycles.Select(x => x.Batch)), exemptionCycles.Sum(x => x.Findings), exemptionCycles.Sum(x => x.Requested),
                exemptionCycles.Sum(x => x.NoScope), exemptionCycles.Sum(x => x.Blocked), exemptionCycles.Sum(x => x.Errors),
                exemptionCycles.Sum(x => x.AlreadyRequested), exemptionCycles.Sum(x => x.SimulatedNotSent), exemptionCycles.Sum(x => x.CheckedOk),
                exemptionCycles.SelectMany(x => x.Causes).GroupBy(kv => kv.Key).ToDictionary(g => g.Key, g => g.Sum(kv => kv.Value)),
                [.. exemptionCycles.SelectMany(x => x.ToCheck)], system.ExemptionApprover,
                [.. exemptionCycles.SelectMany(x => x.Reasons).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
                date == default ? null : Local(date), string.Join(", ", exemptionCycles.Select(x => $"exemption-batches/{x.Batch}/")),
                cycles.Count > 1
                    ? [.. exemptionCycles.Select(x => new ReportData.ExemptionCycle(x.Cycle, x.Batch, x.Findings, x.Requested, x.NoScope))]
                    : []);
        }

        // ---- itens (de todos os ciclos; "ciclo" identifica a sessão de origem)
        var rows = new List<Row>();
        var activatedByRequest = new Dictionary<string, int>(StringComparer.Ordinal);
        var manual = new List<ReportData.ManualApplication>();
        var activatedSources = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in cycles)
        {
            foreach (var i in c.Data.Items)
            {
                var s = Solution(i.Status);
                string sis = "", req = "";
                var history = i.Application?.History ?? [];
                var obs = i.Obs ?? "";
                if (history.Count > 0)
                {
                    var h = history[^1];
                    sis = h.Result ?? "";
                    req = h.Request ?? "";
                    if (sis == "ATIVADO")
                    {
                        activatedByRequest[req] = activatedByRequest.GetValueOrDefault(req) + 1;
                        activatedSources.Add(i.Preparation?.Source ?? i.ObjectName);
                    }
                    else
                    {
                        var mm = ManualRx().Match(obs);
                        manual.Add(new(i.N, i.ObjectName, i.ObjectType, mm.Success ? mm.Groups[1].Value.Trim() : $"aplicação {sis}"));
                        sis = "Aplicação manual";
                    }
                }
                else if (s is "KB" or "Análise" && obs.Contains("[APLICAÇÃO MANUAL", StringComparison.Ordinal))
                {
                    var mm = ManualRx().Match(obs);
                    sis = "Aplicação manual";
                    manual.Add(new(i.N, i.ObjectName, i.ObjectType, mm.Success ? mm.Groups[1].Value.Trim() : ""));
                }
                else if (i.ObjectType == "SSFO" && s is "KB" or "Análise")
                {
                    sis = "Aplicação manual";
                    manual.Add(new(i.N, i.ObjectName, i.ObjectType, "Smart Form (transação SMARTFORMS)"));
                }
                else if (s == "Isenção")
                    sis = exemptionStatus.GetValueOrDefault((c.N, i.N), "");
                rows.Add(new Row
                {
                    N = i.N, Cycle = c.N, Obj = i.ObjectName, Type = i.ObjectType, Check = (i.Check ?? "").Replace("S/4HANA: ", ""),
                    Msg = i.Message ?? "", Ref = i.RefObject ?? "", Sol = s, Sis = sis, Req = req, Obs = obs,
                    Date = i.StatusDate is null ? "" : Iso(i.StatusDate.Value),
                    Hist = [.. i.StatusHistory.Select(h => new ReportData.HistoryEntry(Iso(h.At), h.Status))], Item = i,
                });
            }
        }
        var totalObjects = rows.Select(r => r.Key).Distinct().Count();
        var size = cycles.ToDictionary(c => c.N, c => (Findings: rows.Count(r => r.Cycle == c.N), Objects: rows.Where(r => r.Cycle == c.N).Select(r => r.Key).Distinct().Count()));

        // ---- evolução (pontos do burndown): lotes de decisões gravadas, separados por pausas > 10 min
        var points = new List<ReportData.EvolutionPoint>();
        foreach (var c in cycles)
        {
            var its = rows.Where(r => r.Cycle == c.N).ToList();
            var events = its.SelectMany(r => EventsOf(r)).OrderBy(e => e.At).ToList();
            if (events.Count == 0)
                continue;
            var lots = new List<(DateTimeOffset First, DateTimeOffset Last)>();
            foreach (var e in events)
            {
                if (lots.Count > 0 && (e.At - lots[^1].Last).TotalSeconds <= ProcessHistoryBuilder.PauseSeconds)
                    lots[^1] = (lots[^1].First, e.At);
                else
                    lots.Add((e.At, e.At));
            }
            ReportData.EvolutionPoint PointAt(DateTimeOffset t, string label, DateTimeOffset? exclusiveBefore)
            {
                var counts = new Dictionary<string, int>();
                var pendingObjects = new HashSet<string>();
                foreach (var r in its)
                {
                    var status = "";
                    foreach (var e in EventsOf(r).Where(e => exclusiveBefore is null ? e.At <= t : e.At < exclusiveBefore).OrderBy(e => e.At))
                        status = e.Status;
                    var sol = Solution(status);
                    counts[sol] = counts.GetValueOrDefault(sol) + 1;
                    if (sol == "Pendente")
                        pendingObjects.Add(r.Key);
                }
                return new ReportData.EvolutionPoint(t.ToUnixTimeMilliseconds(), label, counts.GetValueOrDefault("KB"), counts.GetValueOrDefault("Análise"),
                    counts.GetValueOrDefault("Isenção"), counts.GetValueOrDefault("Cancelado"), counts.GetValueOrDefault("Pendente"), pendingObjects.Count,
                    c.N, size[c.N].Findings, size[c.N].Objects);
            }
            points.Add(PointAt(lots[0].First, Rot(lots[0].First) + " · início", lots[0].First));
            foreach (var lot in lots)
                points.Add(PointAt(lot.Last, Rot(lot.Last), null));
        }
        var evolutionSources = new List<string>();
        if (points.Count > 0)
            evolutionSources.Add("remediation_items.status_history (lotes de decisões separados por pausas > 10 min)");
        else
            warnings.Add("Sem evolução registrada (histórico de status dos itens): o burndown terá só a situação atual");

        // ---- KB e versões
        var kb = new ReportData.KbInfo(0,
            rows.Where(r => r.Item.Status == RemediationStatus.ConfirmedAi && r.Item.Proposal?.KbCardId is not null)
                .Select(r => r.Item.Proposal!.KbCardId!).Distinct(StringComparer.Ordinal).Count(),
            null, input.Runs.Count(r => r.Run.Kind == RunKind.Apply && r.Run.Status == RunStatus.Completed));
        var versions = new[] { "atc-extracao", "atc-remediacao", "atc-isencao" }.ToDictionary(s => s,
            s => new ReportData.SkillVersion(null, input.SkillVersions?.GetValueOrDefault(s)));

        // ---- custos: histórico de processo (execuções agrupadas por alvo) por etapa
        var processHistory = ProcessHistoryBuilder.Build(input.Runs);
        var phases = new List<Phase>();
        var clockInstead = new List<string>();
        foreach (var module in new[] { ProcessModule.Extraction, ProcessModule.Remediation, ProcessModule.Exemption })
        {
            foreach (var h in processHistory.Where(x => x.Module == module))
            {
                var net = h.NetSeconds;
                if (net == 0 && h.ClockSeconds > 0)
                {
                    clockInstead.Add($"{h.Stage} {Local(h.End)} ({h.ClockSeconds / 60} min de relógio)");
                    net = h.ClockSeconds;
                }
                var cic = CycleOf(h.End);
                var stage = cycles.Count > 1 ? $"{h.Stage} · ciclo {cic}" : h.Stage;
                var f = phases.FirstOrDefault(p => p.Stage == stage);
                if (f is null)
                {
                    f = new Phase { Stage = stage, Cycle = cic, Start = h.Start, End = h.End };
                    phases.Add(f);
                }
                f.Start = h.Start < f.Start ? h.Start : f.Start;
                f.End = h.End > f.End ? h.End : f.End;
                f.Sessions += 1;
                f.Net += net;
                f.Total += h.TotalTokens;
                f.Output += h.OutputTokens;
                f.InputWrite += h.InputTokens;
            }
        }
        if (clockInstead.Count > 0)
            warnings.Add("Execuções sem tempo líquido medido (ou medido 0): usado o tempo de relógio, que é maior ou igual: " + string.Join(", ", clockInstead));
        string Period(Phase p) => $"{Rot(p.Start)} – {Rot(p.End)}";
        var costPhases = phases.Select(p => new ReportData.CostPhase(p.Stage, p.Cycle, p.Start.ToString("yyyy-MM-ddTHH:mmzzz", CultureInfo.InvariantCulture),
            p.End.ToString("yyyy-MM-ddTHH:mmzzz", CultureInfo.InvariantCulture), p.Sessions, p.Net, p.Total, p.Output, p.CacheRead, p.InputWrite, Period(p))).ToList();
        string? totalPeriod = phases.Count > 0 ? $"{Rot(phases[0].Start)} – {Rot(phases[^1].End)}" : null;
        long? calendarHours = phases.Count > 0 ? (long)Math.Round((phases[^1].End - phases[0].Start).TotalHours) : null;
        var costTotal = new ReportData.CostTotal(phases.Sum(p => p.Sessions), phases.Sum(p => p.Net), phases.Sum(p => p.Total), phases.Sum(p => p.Output),
            phases.Sum(p => p.CacheRead), phases.Sum(p => p.InputWrite), totalPeriod, calendarHours);
        var remediationNet = phases.Where(p => p.Stage.StartsWith("Remediação", StringComparison.Ordinal)).Sum(p => p.Net);
        var costs = new ReportData.CostsInfo(costPhases, costTotal, remediationNet,
            remediationNet != 0 && rows.Count > 0 ? (long)Math.Round((double)remediationNet / rows.Count, MidpointRounding.ToEven) : null,
            remediationNet != 0 && totalObjects > 0 ? (long)Math.Round((double)remediationNet / totalObjects, MidpointRounding.ToEven) : null,
            "execuções (Run) agrupadas por alvo: tempo líquido sem pausas > 10 min; tokens de entrada e saída", input.IncludeReportRuns, clockInstead);

        // ---- execução do processo: chamadas de IA por uso e modelo (no lugar dos agentes dos transcripts)
        var processRuns = new List<ReportData.ProcessRun>();
        var byType = new Dictionary<string, int>();
        var byModel = new Dictionary<string, int>();
        var tokensByModel = new Dictionary<string, long>();
        foreach (var module in new[] { ProcessModule.Extraction, ProcessModule.Remediation, ProcessModule.Exemption, ProcessModule.Scan })
        {
            foreach (var h in processHistory.Where(x => x.Module == module))
            {
                var agents = h.ByModel.Select(m => new ReportData.ProcessAgent(null, m.Usage, $"{m.Calls} chamada(s) de IA", [m.Model], 0, m.InputTokens + m.OutputTokens, null)).ToList();
                foreach (var a in agents)
                {
                    byType[a.Type ?? "?"] = byType.GetValueOrDefault(a.Type ?? "?") + 1;
                    var mk = string.Join(" + ", a.Models);
                    byModel[mk] = byModel.GetValueOrDefault(mk) + 1;
                    tokensByModel[mk] = tokensByModel.GetValueOrDefault(mk) + a.Tokens;
                }
                var par = h.ParallelPeriods.Select(p => new ReportData.ProcessParallel(Local(p.Start), Local(p.End), p.Parallelism, p.Parallelism, null, null, null)).ToList();
                var skill = module switch { ProcessModule.Extraction => "atc-extracao", ProcessModule.Remediation => "atc-remediacao", ProcessModule.Exemption => "atc-isencao", _ => "atc-extracao-mignow" };
                processRuns.Add(new ReportData.ProcessRun(h.Stage, "runs", Local(h.Start), Local(h.End), [], null, new Dictionary<string, int> { [skill] = h.Runs },
                    agents, agents.Count, 0, agents.Sum(a => a.Tokens), par, 0));
            }
        }
        var process = new ReportData.ProcessInfo(processRuns, [],
            new ReportData.ProcessTotals(byType.Values.Sum(), byType.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value),
                byModel.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value),
                tokensByModel.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value), 0, 0, processRuns.Count, 0),
            "execuções (Run) e chamadas de IA (RunAiCall) do banco");

        // ---- validação
        foreach (var c in cycles)
        {
            var cc = rows.Where(r => r.Cycle == c.N).GroupBy(r => r.Sol).ToDictionary(g => g.Key, g => g.Count());
            var pc = points.Where(p => p.Cycle == c.N).ToList();
            var name = cycles.Count > 1 ? $"ciclo {c.N} ({c.Source})" : "sessão atual";
            if (pc.Count > 0)
            {
                var u = pc[^1];
                foreach (var (k, v) in new[] { ("KB", u.Kb), ("Análise", u.Analysis), ("Isenção", u.Exemption), ("Pendente", u.Pending) })
                    if (v != cc.GetValueOrDefault(k))
                        warnings.Add($"Último ponto da evolução ({u.Label}) tem {k} = {v}, {name} = {cc.GetValueOrDefault(k)}");
            }
            else if (cycles.Count > 1)
                warnings.Add($"Ciclo {c.N} sem pontos de evolução: o burndown não mostra esse ciclo");
            foreach (var p in pc)
            {
                var sum = p.Kb + p.Analysis + p.Exemption + p.Cancelled + p.Pending;
                if (sum != p.Total)
                    warnings.Add($"Ponto {p.Label}: soma {sum} ≠ {p.Total} itens do ciclo {c.N}");
            }
            var li = exemptionCycles.FirstOrDefault(x => x.Cycle == c.N);
            if (li is not null && li.Findings != li.Requested + li.NoScope + li.Blocked + li.Errors + li.AlreadyRequested + li.SimulatedNotSent)
                warnings.Add($"Isenção: a soma das situações não fecha com o lote {li.Batch}");
            if (li is not null && cc.GetValueOrDefault("Isenção") != li.Findings)
                warnings.Add($"Isenção: {cc.GetValueOrDefault("Isenção")} itens com status Isenção no {name} e {li.Findings} findings no lote {li.Batch}");
        }

        // ---- ciclos: resumo e findings que voltaram (mesmo tipo, objeto, check, referência e mensagem de um ciclo anterior)
        static (string, string, string, string, string) KeyF(Row x) => (x.Type, x.Obj, x.Check, x.Ref, x.Msg);
        var cycleSummaries = new List<ReportData.CycleSummary>();
        foreach (var c in cycles)
        {
            var its = rows.Where(r => r.Cycle == c.N).ToList();
            var before = new Dictionary<(string, string, string, string, string), string>();
            foreach (var x in rows.Where(r => r.Cycle < c.N))
                before[KeyF(x)] = x.Sol;
            var back = its.Where(x => before.ContainsKey(KeyF(x))).GroupBy(x => before[KeyF(x)]).ToDictionary(g => g.Key, g => g.Count());
            cycleSummaries.Add(new ReportData.CycleSummary(c.N, c.Source, c.Start is null ? null : Local(c.Start.Value), c.Extraction,
                [.. c.Scope.Select(p => p.ToUpperInvariant())], its.Count, its.Select(x => x.Key).Distinct().Count(),
                its.GroupBy(x => x.Sol).ToDictionary(g => g.Key, g => g.Count()), new ReportData.AlreadyTreated(back.Values.Sum(), back)));
        }

        // ---- comparação manual × IA
        var premise = highlights?.Premise;
        var pm = new ReportData.Premises(premise?.ObjectsPerDay ?? input.Settings.ManualObjectsPerDay, premise?.HoursPerDay ?? input.Settings.HoursPerDay,
            premise?.Source ?? (premise is { ObjectsPerDay: not null } or { HoursPerDay: not null } ? "premissa do cliente (complementos do relatório)" : DefaultPremiseSource));
        var compRows = new List<ReportData.ComparisonRow>();
        foreach (var c in cycles)
        {
            var g = rows.Where(r => r.Cycle == c.N).GroupBy(r => r.Key).ToList();
            var treated = g.Count(x => !x.Any(r => r.Sol == "Pendente"));
            var ia = phases.Where(f => f.Cycle == c.N).Sum(f => f.Net);
            var iaRem = phases.Where(f => f.Cycle == c.N && f.Stage.StartsWith("Remediação", StringComparison.Ordinal)).Sum(f => f.Net);
            compRows.Add(new ReportData.ComparisonRow(c.N, treated, g.Count - treated,
                (long)Math.Round(treated / (double)pm.ObjectsPerDay * pm.HoursPerDay * 3600, MidpointRounding.ToEven),
                Math.Round(treated / (double)pm.ObjectsPerDay, 2, MidpointRounding.ToEven), ia, iaRem));
        }
        var tTreated = compRows.Sum(x => x.ObjectsTreated);
        var tManual = compRows.Sum(x => x.ManualSeconds);
        var tIa = compRows.Sum(x => x.AiSeconds);
        var tIaRem = compRows.Sum(x => x.AiRemediationSeconds);
        var comparison = new ReportData.Comparison(pm, compRows,
            new ReportData.ComparisonTotal(tTreated, compRows.Sum(x => x.ObjectsPending), tManual, tIa, tIaRem,
                Math.Round(tTreated / (double)pm.ObjectsPerDay, 2, MidpointRounding.ToEven), tManual - tIa,
                tManual != 0 ? Math.Round((tManual - tIa) / (double)tManual * 100, 1, MidpointRounding.ToEven) : null,
                tIa != 0 ? Math.Round(tManual / (double)tIa, 1, MidpointRounding.ToEven) : null,
                tIaRem != 0 ? Math.Round(tManual / (double)tIaRem, 1, MidpointRounding.ToEven) : null),
            "objetos: sessões dos ciclos; IA: tempo líquido das fases de custo (custos.fases)");
        if (tIa == 0)
            warnings.Add("Comparação manual × IA: sem tempo líquido medido; a seção mostra só a estimativa manual");

        var current = cycles.Count > 0 ? cycles[^1].Data.Session : null;
        var customer = new ReportData.CustomerInfo(input.Customer.Name, system.Sid, system.Client, system.Release, RoleOf(system.ClientCategory), system.Author,
            current?.Request ?? system.DefaultRequest);
        return new ReportData(
            input.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture), customer, extraction, [.. scope.Select(p => p.ToUpperInvariant())],
            cycleSummaries,
            [.. rows.Select(r => new ReportData.ItemRow(r.N, r.Cycle, r.Obj, r.Type, r.Check, r.Msg, r.Ref, r.Sol, r.Sis, r.Req, r.Obs, r.Date, r.Hist))],
            totalObjects, new ReportData.EvolutionInfo(points, evolutionSources),
            new ReportData.ApplicationInfo(activatedByRequest.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value), activatedSources.Count,
                manual, current?.Request ?? system.DefaultRequest),
            exemption, kb, versions, costs, comparison, process,
            new ReportData.ComplementsInfo(highlights?.Highlights ?? [], highlights?.Pending ?? []),
            new ReportData.ValidationInfo(warnings.Count == 0, warnings));
    }

    private static int? PriorityOf(ReportData.ExtractionInfo e, string p) => p switch { "p1" => e.P1, "p2" => e.P2, _ => e.P3 };

    private static string? RoleOf(string? category) => category switch
    {
        "P" => "Produção",
        "T" => "Teste",
        "C" => "Customizing",
        "D" => "Demonstração",
        "E" => "Treinamento",
        "S" => "Referência SAP",
        _ => null,
    };

    private sealed record StatusEvent(DateTimeOffset At, string Status);

    /// <summary>Decisões do item no tempo: o histórico de status (ou, sem histórico, a última decisão com a data do item).</summary>
    private static IEnumerable<StatusEvent> EventsOf(Row r)
    {
        if (r.Item.StatusHistory.Count > 0)
            return r.Item.StatusHistory.Select(h => new StatusEvent(h.At, h.Status));
        if (r.Item.Status != RemediationStatus.Pending && r.Item.StatusDate is not null)
            return [new StatusEvent(r.Item.StatusDate.Value, r.Item.Status)];
        return [];
    }

    [GeneratedRegex(@"\[APLICAÇÃO MANUAL[: ]*([^\]]*)\]")]
    private static partial Regex ManualRx();
}
