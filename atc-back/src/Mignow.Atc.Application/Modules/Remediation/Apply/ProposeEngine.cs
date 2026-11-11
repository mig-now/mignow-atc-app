using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation.Apply.Ai;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Ddic;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Proposta de correção (Etapa 4 da skill): para cada fonte físico (em paralelo, <c>GeneralSettings.Parallelism</c>) e cada objeto referenciado,
/// busca a KB (<c>kb.top</c>), lê o DDIC antes (somente leitura), pede à IA o uso por complexidade (simples → Mechanical, média → Review,
/// complexa e <c>mode=ia</c> → Analysis), valida o JSON, gera os marcadores, monta o fonte novo e o diff (<see cref="SourcePatcher"/>), aplica os
/// alertas da KB e verifica a sintaxe SEM gravar. Grava <c>Proposal</c>/<c>Preparation</c> nos itens (entidades passadas, via <see cref="EngineHost"/>).
/// </summary>
public sealed partial class ProposeEngine(
    IAiGateway ai, IKnowledgeBase kb, ISapReader reader, IDdicReader ddicReader, IDdicWriter ddicWriter, IDateTimeProvider clock, IStringLocalizer<Errors> l)
{
    public const string ModeKb = "kb";
    public const string ModeAi = "ia";
    private const int WindowBefore = 30;
    private const int WindowAfter = 40;
    private const int MaxDdic = 6;

    /// <summary>
    /// Fonte até este tamanho vai inteiro para a IA, com ou sem linha (recomendação da IA, não está no script: o agente da skill lê o arquivo
    /// todo). Assim ela vê declarações, SELECTs e a ordem das chamadas fora da janela da linha. Acima disso: janelas em torno das linhas
    /// (ou só o começo, sem linha), como antes.
    /// </summary>
    private const int FullSourceMaxLines = 1500;

    private sealed class Unit
    {
        public required string Key { get; init; }
        public required string SourceName { get; init; }
        public required string Uri { get; init; }
        public required string Text { get; init; }
        public SourceCache? Cache { get; init; }
        public DdicKind? DdlKind { get; init; }
        public bool Ddl => DdlKind is not null;
        public List<(RemediationItem Item, int? Line)> Items { get; } = [];
    }

    private sealed class Counters
    {
        public int Fix, Exemption, Cancel, Pending, FromKb, FromAi, WithAlerts, AiFailures;
        public readonly ConcurrentBag<FactCandidate> Facts = [];
    }

    public async Task<ProposeSummary> RunAsync(ProposeInput input, EngineHost host)
    {
        var counters = new Counters();
        var resolver = new ItemSourceResolver(input.Caches, input.Columns, input.Locations);
        var units = new Dictionary<string, Unit>();
        var ddlCache = new Dictionary<string, Unit?>();
        foreach (var item in input.Items)
        {
            var (cache, line) = resolver.Resolve(item);
            Unit? unit = null;
            if (cache is not null)
            {
                var key = cache.AdtUri.ToLowerInvariant();
                if (!units.TryGetValue(key, out unit))
                    units[key] = unit = new Unit { Key = key, SourceName = cache.IncludeName, Uri = cache.AdtUri, Text = cache.Source, Cache = cache };
            }
            else if (item.ObjectType.ToUpperInvariant() is "STRU" or "TABL")
            {
                var kind = item.ObjectType.Equals("TABL", StringComparison.OrdinalIgnoreCase) ? DdicKind.Table : DdicKind.Structure;
                var key = $"ddic://{item.ObjectType.ToUpperInvariant()}/{item.ObjectName.ToUpperInvariant()}";
                if (!ddlCache.TryGetValue(key, out unit))
                {
                    var state = await ddicWriter.ReadObjectAsync(host.Connection, kind, item.ObjectName, host.Ct);
                    ddlCache[key] = unit = state.IsSuccess && state.Value is { Exists: true, Source: { } src }
                        ? new Unit { Key = key, SourceName = item.ObjectName.ToUpperInvariant(), Uri = key, Text = src, DdlKind = kind }
                        : null;
                    if (unit is not null)
                        units[key] = unit;
                }
            }
            if (unit is null)
            {
                await Record(host, counters, item, Pending(ProposalSource(input.Mode), null, l.Text("Apply.Propose.NoSource"), clock.UtcNow), null, input.Mode);
                continue;
            }
            unit.Items.Add((item, line));
        }

        var kbCache = new ConcurrentDictionary<string, Lazy<Task<(KbLookupResult Lookup, IReadOnlyList<KbFact> Facts)>>>();
        var ddicCache = new ConcurrentDictionary<string, Lazy<Task<DdicDefinition?>>>();
        var done = 0;
        var total = input.Items.Count;
        await host.ProgressAsync(0, Math.Max(1, total));
        await Parallel.ForEachAsync(units.Values, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, host.Settings.Parallelism), CancellationToken = host.Ct },
            async (unit, _) =>
            {
                foreach (var group in unit.Items.GroupBy(i => $"{(i.Item.RefObject ?? string.Empty).ToUpperInvariant()}|{(i.Item.RefType ?? string.Empty).ToUpperInvariant()}"))
                {
                    if (await host.IsCancelledAsync())
                        return;
                    await ProposeGroupAsync(input, host, resolver, unit, [.. group], kbCache, ddicCache, counters);
                    var n = Interlocked.Add(ref done, group.Count());
                    await host.ProgressAsync(Math.Min(n, total), Math.Max(1, total));
                }
            });
        await host.SaveAsync();
        return new ProposeSummary(input.Items.Count, counters.Fix, counters.Exemption, counters.Cancel, counters.Pending, counters.FromKb, counters.FromAi, counters.WithAlerts,
            counters.AiFailures, [.. counters.Facts.OrderBy(f => f.N)]);
    }

    private static string ProposalSource(string mode) => mode == ModeKb ? ItemProposal.OriginKb : ItemProposal.OriginAi;

    private ItemProposal Pending(string origin, string? cardId, string justification, DateTimeOffset at) =>
        new(origin, cardId, [], null, null, null, [], null, null, null, null, justification, ItemProposal.ActionPending, null, at);

    private async Task ProposeGroupAsync(
        ProposeInput input, EngineHost host, ItemSourceResolver resolver, Unit unit, List<(RemediationItem Item, int? Line)> group,
        ConcurrentDictionary<string, Lazy<Task<(KbLookupResult, IReadOnlyList<KbFact>)>>> kbCache, ConcurrentDictionary<string, Lazy<Task<DdicDefinition?>>> ddicCache,
        Counters counters)
    {
        var first = group[0].Item;
        var reference = (first.RefObject ?? string.Empty).Trim();
        var refType = first.RefType;
        var lines = PyText.SplitLines(unit.Text);

        // KB: top 3 (contribuições primeiro) e fatos do objeto referenciado.
        KbLookupResult lookup = new([], 0);
        IReadOnlyList<KbFact> facts = [];
        if (reference.Length > 0)
        {
            var key = $"{reference.ToUpperInvariant()}|{(refType ?? string.Empty).ToUpperInvariant()}";
            (lookup, facts) = await kbCache.GetOrAdd(key, _ => new Lazy<Task<(KbLookupResult, IReadOnlyList<KbFact>)>>(async () =>
                (await kb.LookupAsync(reference, refType, 3, host.Ct), await kb.FactsAsync(reference, host.Ct)))).Value;
        }
        var cards = input.Mode == ModeKb ? lookup.Cards : [];
        if (input.Mode == ModeKb && cards.Count == 0)
        {
            foreach (var (item, _) in group)
                await Record(host, counters, item, Pending(ItemProposal.OriginKb, null, l.Text("Apply.Propose.NoKbSolution"), clock.UtcNow), null, input.Mode);
            return;
        }

        var usages = reference.Length > 0 && unit.Cache is not null
            ? (resolver.Index.Locate(unit.Cache.ObjectName, reference) ?? []).Where(h => h.Source == unit.Cache.IncludeName).Take(10).ToList()
            : [];
        var complexity = input.Mode == ModeAi ? "complexa" : cards[0].Complexity;
        if (usages.Count == 0 && group.All(g => g.Line is null) && complexity == "simples")
            complexity = "media";
        var usage = input.Mode == ModeAi ? AiUsage.Analysis : complexity switch { "simples" => AiUsage.Mechanical, "media" => AiUsage.Review, _ => AiUsage.Analysis };

        var ddic = new List<ProposalDdic>();
        if (usage != AiUsage.Mechanical)
            foreach (var name in DdicNames(reference, refType, cards).Take(MaxDdic))
            {
                var def = await ddicCache.GetOrAdd(name, n => new Lazy<Task<DdicDefinition?>>(async () =>
                {
                    var r = await ddicReader.ReadDefinitionAsync(host.Connection, n, null, host.Ct);
                    return r.IsSuccess ? r.Value : null;
                })).Value;
                if (def is { Found: true, Kind: { } k })
                    ddic.Add(new ProposalDdic(def.Name, k.ToString(), def.Source!));
            }

        var task = new ProposalTask(
            input.Mode, group[0].Item.ObjectName, group[0].Item.ObjectType, unit.SourceName, unit.Ddl ? "ddl" : "abap", reference, refType, null,
            [.. group.Select(g => new ProposalTaskItem(g.Item.N, g.Line ?? usages.FirstOrDefault()?.Line, g.Item.Check, g.Item.Message, g.Item.Note, g.Item.Classification, g.Item.Text))],
            [.. cards.Select(c => new ProposalKbCard(c.Id, c.Occurrences, c.Pattern, c.Original, c.New, c.Alerts, c.Complexity, c.ReplacedBy))],
            [.. facts.Select(f => new ProposalFact(f.Id, f.Text))],
            [.. usages.Select(u => new ProposalUsage(u.Line, u.Excerpt))],
            Windows(lines, [.. group.Select(g => g.Line).Concat(usages.Select(u => (int?)u.Line))]), ddic, []);

        string? model = null;
        var tokens = 0;
        var itemRef = $"item {string.Join(',', group.Select(g => g.Item.N))}";
        var request = new AiRequest(usage, RemediationPurposes.Propose, ProposalPrompts.System, ProposalPrompts.User(task), ProposalPrompts.Schema,
            usage switch { AiUsage.Mechanical => AiEffort.Low, AiUsage.Review => AiEffort.Medium, _ => AiEffort.High });
        var response = await ai.CompleteAsync<ProposalAiResponse>(request, u =>
        {
            model = u.Model;
            tokens += u.InputTokens + u.OutputTokens;
            host.AddAiUsage(u, usage, itemRef);
        }, host.Ct);
        if (response.IsFailure)
        {
            Interlocked.Increment(ref counters.AiFailures);
            await host.LogAsync(EventLevel.Warning, response.Error.Code, null, itemRef);
            foreach (var (item, _) in group)
                await Record(host, counters, item, Pending(ProposalSource(input.Mode), null, l.Text("Apply.Propose.AiFailed", response.Error.Code), clock.UtcNow), null, input.Mode);
            return;
        }

        var byN = (response.Value.Items ?? []).GroupBy(i => i.N).ToDictionary(g => g.Key, g => g.First());
        var built = new Dictionary<int, (ItemProposal Proposal, ItemPreparation? Prep)>();
        var perItemTokens = Math.Max(1, tokens / group.Count);
        foreach (var (item, line) in group.OrderBy(g => byN.TryGetValue(g.Item.N, out var a) && a.SameBlockAs is not null ? 1 : 0))
        {
            if (!byN.TryGetValue(item.N, out var ai1))
            {
                built[item.N] = (Pending(ProposalSource(input.Mode), null, l.Text("Apply.Propose.AiMissing"), clock.UtcNow), null);
                continue;
            }
            foreach (var f in ai1.FactCandidates ?? [])
                counters.Facts.Add(new FactCandidate(item.N, f.Element, f.Fact, f.Proof, f.Impact));
            IReadOnlyList<ProposalFactCandidate>? facts1 = ai1.FactCandidates is { Count: > 0 } fc
                ? [.. fc.Select(f => new ProposalFactCandidate(f.Element, f.Fact, f.Proof, f.Impact))]
                : null;
            if (ai1.SameBlockAs is { } main && built.TryGetValue(main, out var shared) && shared.Proposal.Action == ItemProposal.ActionFix)
            {
                built[item.N] = (shared.Proposal with
                {
                    Justification = $"{ai1.Justification} ({l.Text("Apply.Propose.SameBlock", main)})", Tokens = perItemTokens, FactCandidates = facts1,
                }, shared.Prep);
                continue;
            }
            var (proposal1, prep1) = await BuildAsync(input, host, unit, item, ai1, cards, model, perItemTokens);
            built[item.N] = (proposal1 with { FactCandidates = facts1 }, prep1);
        }
        foreach (var (item, _) in group)
            await Record(host, counters, item, built[item.N].Proposal, built[item.N].Prep, input.Mode);
    }

    private async Task<(ItemProposal, ItemPreparation?)> BuildAsync(
        ProposeInput input, EngineHost host, Unit unit, RemediationItem item, ProposalAiItem ai1, IReadOnlyList<KbCard> cards, string? model, int tokens)
    {
        var at = clock.UtcNow;
        var origin = string.Equals(ai1.Origin, ItemProposal.OriginKb, StringComparison.OrdinalIgnoreCase) && ai1.KbCardId is not null && cards.Any(c => c.Id == ai1.KbCardId)
            ? ItemProposal.OriginKb : ItemProposal.OriginAi;
        var cardId = origin == ItemProposal.OriginKb ? ai1.KbCardId : null;
        var action = ai1.Action?.ToLowerInvariant() ?? ItemProposal.ActionPending;
        ItemProposal Simple(string a, string? why) =>
            new(origin, cardId, [], null, null, null, [], null, null, model, tokens, why ?? ai1.Justification, a, null, at);
        if (action != ItemProposal.ActionFix)
            return (Simple(action is ItemProposal.ActionExemption or ItemProposal.ActionCancel ? action : ItemProposal.ActionPending, null), null);

        List<ProposalBlock> blocks = [];
        foreach (var p in ai1.Patches ?? [])
            blocks.Add(new ProposalBlock([.. p.Before.Select(x => x.TrimEnd('\r'))], [.. p.After.Select(x => x.TrimEnd('\r'))], p.Line, p.Kind, string.IsNullOrWhiteSpace(p.RelatedObject) ? null : p.RelatedObject.ToUpperInvariant()));
        if (blocks.Count == 0)
            return (Simple(ItemProposal.ActionPending, l.Text("Apply.Propose.NoPatches")), null);

        var main = blocks.Where(b => b.RelatedObject is null || b.RelatedObject == item.ObjectName.ToUpperInvariant()).ToList();
        var related = blocks.Where(b => b.RelatedObject is not null && b.RelatedObject != item.ObjectName.ToUpperInvariant()).GroupBy(b => b.RelatedObject!).ToList();
        var patch = PatchBlocks(unit.Text, unit.SourceName, main, input.AuthorTag, input.Date, unit.Ddl);
        if (!patch.IsOk)
            return (Simple(ItemProposal.ActionPending, l.Text("Apply.Propose.PatchFailed", patch.Code!, patch.Arg ?? string.Empty)), null);

        var relatedPreps = new List<RelatedPreparation>();
        foreach (var g in related)
        {
            var relatedCache = input.Caches.Where(c => string.Equals(c.ObjectName, g.Key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => string.Equals(c.IncludeName, g.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (relatedCache is null)
                return (Simple(ItemProposal.ActionPending, l.Text("Apply.Propose.RelatedNoSource", g.Key)), null);
            var rp = PatchBlocks(relatedCache.Source, relatedCache.IncludeName, [.. g], input.AuthorTag, input.Date, false);
            if (!rp.IsOk)
                return (Simple(ItemProposal.ActionPending, l.Text("Apply.Propose.PatchFailed", rp.Code!, rp.Arg ?? string.Empty)), null);
            relatedPreps.Add(new RelatedPreparation(g.Key, RelatedType(g.Key), relatedCache.IncludeName, relatedCache.AdtUri, rp.Outcome!.NewSource, rp.Outcome.Diff));
        }

        var alerts = new List<string>();
        if (cardId is not null && cards.FirstOrDefault(c => c.Id == cardId) is { } card)
            alerts.AddRange(card.Alerts);
        alerts.AddRange(ProposalAlerts.Of(string.Join('\n', main.SelectMany(b => b.After)), string.Join('\n', main.SelectMany(b => b.Before))).Where(a => !alerts.Contains(a)));

        bool? syntaxOk = null;
        List<string>? syntaxMessages = null;
        if (!unit.Ddl && unit.Cache is not null)
        {
            var objUri = SapUris.ObjectUri(unit.Uri);
            var check = await reader.CheckSourceAsync(host.Connection, objUri, patch.Outcome!.NewSource!, SapUris.ActivationContext(objUri, item.ObjectType, item.ObjectName), host.Ct);
            if (check.IsSuccess)
            {
                syntaxMessages = [.. check.Value.Select(m => $"{m.Severity}: {m.Text}" + (m.Line is { } n ? $" ({n})" : string.Empty))];
                syntaxOk = !check.Value.Any(m => m.Severity is "E" or "A" or "X");
            }
            else
                syntaxMessages = [l.Text("Apply.Propose.SyntaxUnavailable", check.Error.Code)];
        }
        var marked = unit.Ddl ? null : ProposalBlocks.MarkedText(main, input.AuthorTag, input.Date);
        var proposal = new ItemProposal(origin, cardId, blocks, marked, patch.Outcome!.NewSource, patch.Outcome.Diff, alerts, syntaxOk, syntaxMessages, model, tokens,
            ai1.Justification, ItemProposal.ActionFix, null, at);
        var prep = new ItemPreparation(unit.SourceName, unit.Uri, patch.Outcome.Ranges, patch.Outcome.NewSource, patch.Outcome.Diff, relatedPreps.Count == 0 ? null : relatedPreps, at);
        return (proposal, prep);
    }

    private static string RelatedType(string name) => name.Contains("_IF_", StringComparison.OrdinalIgnoreCase) || name.Contains("ZIF_", StringComparison.OrdinalIgnoreCase) || name.StartsWith("IF_", StringComparison.OrdinalIgnoreCase) ? "INTF" : "CLAS";

    private readonly record struct PatchAttempt(bool IsOk, PatchOutcome? Outcome, string? Code, string? Arg);

    private static PatchAttempt PatchBlocks(string source, string name, IReadOnlyList<ProposalBlock> blocks, string authorTag, DateOnly date, bool ddl)
    {
        List<PatchSpec> specs = [];
        foreach (var b in blocks)
        {
            var (spec, code, arg) = ProposalBlocks.ToSpec(b, authorTag, date, ddl);
            if (spec is null)
                return new PatchAttempt(false, null, code, arg);
            specs.Add(spec);
        }
        var outcome = SourcePatcher.Patch(source, specs, name);
        return outcome.IsOk ? new PatchAttempt(true, outcome, null, null) : new PatchAttempt(false, null, outcome.ErrorCode, string.Join(",", outcome.Occurrences));
    }

    private async Task Record(EngineHost host, Counters c, RemediationItem item, ItemProposal proposal, ItemPreparation? prep, string mode)
    {
        _ = mode;
        await host.MutateAsync(() =>
        {
            item.SetProposal(proposal);
            item.SetPreparation(prep);
        });
        lock (c)
        {
            switch (proposal.Action)
            {
                case ItemProposal.ActionFix: c.Fix++; break;
                case ItemProposal.ActionExemption: c.Exemption++; break;
                case ItemProposal.ActionCancel: c.Cancel++; break;
                default: c.Pending++; break;
            }
            if (proposal.Origin == ItemProposal.OriginKb)
                c.FromKb++;
            else
                c.FromAi++;
            if (proposal.Alerts.Count > 0)
                c.WithAlerts++;
        }
    }

    private static IEnumerable<string> DdicNames(string reference, string? refType, IReadOnlyList<KbCard> cards)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        var names = new List<string>();
        if (reference.Length > 0 && refType is "TABL" or "STRU" or "DTEL" or "FUNC")
            names.Add(reference);
        foreach (var c in cards)
        {
            names.AddRange(FunctionCall().Matches(c.New).Select(m => m.Groups[1].Value));
            names.AddRange(BapiType().Matches(c.New).Select(m => m.Groups[1].Value));
        }
        return names.Select(n => n.ToUpperInvariant()).Where(seen.Add);
    }

    /// <summary>Fonte pequeno (até <see cref="FullSourceMaxLines"/>) inteiro; maior, janelas em torno das linhas de uso (sobrepostas viram uma só).</summary>
    private static List<ProposalWindow> Windows(List<string> lines, IEnumerable<int?> centers)
    {
        if (lines.Count <= FullSourceMaxLines)
            return [new ProposalWindow(1, string.Join('\n', lines))];
        var ranges = centers.OfType<int>().Distinct().Order().Select(c => (From: Math.Max(1, c - WindowBefore), To: Math.Min(lines.Count, c + WindowAfter))).ToList();
        if (ranges.Count == 0)
            ranges.Add((1, Math.Min(lines.Count, WindowBefore + WindowAfter)));
        List<(int From, int To)> merged = [];
        foreach (var r in ranges)
            if (merged.Count > 0 && r.From <= merged[^1].To + 1)
                merged[^1] = (merged[^1].From, Math.Max(merged[^1].To, r.To));
            else
                merged.Add(r);
        return [.. merged.Select(m => new ProposalWindow(m.From, string.Join('\n', lines.Skip(m.From - 1).Take(m.To - m.From + 1))))];
    }

    [GeneratedRegex(@"CALL\s+FUNCTION\s+'([A-Za-z0-9_/]+)'", RegexOptions.IgnoreCase)]
    private static partial Regex FunctionCall();

    [GeneratedRegex(@"\bTYPE\s+(?:STANDARD\s+TABLE\s+OF\s+)?(BAPI[A-Za-z0-9_]+)", RegexOptions.IgnoreCase)]
    private static partial Regex BapiType();
}
