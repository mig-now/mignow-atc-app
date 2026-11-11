using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Dados que os passos carregam (entidades rastreadas pelo contexto da execução, sob a trava do banco).</summary>
public sealed record StepData(RemediationSession Session, List<RemediationItem> Items, List<SourceCache> Caches)
{
    public IReadOnlyList<string> Columns => System.Text.Json.JsonSerializer.Deserialize<string[]>(Session.CsvColumnsJson) ?? [];

    public ApplyInput ToApplyInput(RunContext ctx, bool allowTableStructure, IReadOnlyList<DdicCreateParameter>? create) =>
        new(Session.Request, ChangeMarker.AuthorTag(Session.Author), ctx.SapConnection.User, Items, Caches, allowTableStructure,
            (create ?? []).Select(c => (Item: Items.FirstOrDefault(i => i.Guid == c.ItemId), c)).Where(x => x.Item is not null)
            .ToDictionary(x => x.Item!.N, x => new DdicCreation(x.c.Package, x.c.Description)));

    /// <summary>
    /// Porte do <c>extracao_por_finding</c>: Finding ID (<c>quickfixInfo</c>) → <c>location</c> do finding na extração mais recente do ambiente
    /// que o tem (fonte físico e <c>#start=linha</c>). O CSV da sessão no layout da remediação não traz a linha; sem isso a proposta só via o
    /// começo do fonte.
    /// </summary>
    public Task<IReadOnlyDictionary<string, string>> LocationsAsync(RunContext ctx) =>
        ctx.WithDbAsync<IReadOnlyDictionary<string, string>>(async () =>
        {
            var ids = Items.Select(i => i.FindingId).OfType<string>().Where(f => f.Length > 0).Distinct().ToArray();
            if (ids.Length == 0)
                return new Dictionary<string, string>();
            var systemId = Session.SapSystemId;
            var extractions = await ctx.Store.ListAsync(ctx.Store.Read<AtcExtraction>().Where(e => e.SapSystemId == systemId)
                .Select(e => new { e.Id, e.ExecutedAtUtc, e.ExtractedAt }), ctx.Ct);
            var order = extractions.OrderByDescending(e => e.ExecutedAtUtc).ThenByDescending(e => e.ExtractedAt).ThenByDescending(e => e.Id)
                .Select((e, k) => (e.Id, k)).ToDictionary(x => x.Id, x => x.k);
            var extractionIds = order.Keys.ToArray();
            var findings = await ctx.Store.ListAsync(ctx.Store.Read<AtcFinding>()
                .Where(f => extractionIds.Contains(f.ExtractionId) && f.FindingId != null && ids.Contains(f.FindingId))
                .Select(f => new { f.FindingId, f.ExtractionId, f.AllColumnsJson }), ctx.Ct);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in findings.OrderBy(f => order[f.ExtractionId]))
                if (!result.ContainsKey(f.FindingId!) && ExtractionFiles.ParseRow(f.AllColumnsJson).TryGetValue("location", out var loc) && loc.Length > 0)
                    result[f.FindingId!] = loc;
            return result;
        });

    /// <summary>
    /// Itens do run: <paramref name="itemIds"/> e/ou os grupos (<c>C|REF|ROBT|forma</c> → referenciado e tipo); sem nenhum, todos que passam
    /// no <paramref name="filter"/>. Carrega os fontes do ambiente (todos, para achar também os dos objetos relacionados).
    /// </summary>
    public static Task<StepData?> LoadAsync(RunContext ctx, Guid sessionId, IReadOnlyList<Guid>? itemIds, IReadOnlyList<string>? groupKeys, Func<RemediationItem, bool> filter) =>
        ctx.WithDbAsync(async () =>
        {
            var session = await ctx.Store.FirstOrDefaultAsync(ctx.Store.Query<RemediationSession>().Where(s => s.Guid == sessionId), ctx.Ct);
            if (session is null)
                return null;
            var id = session.Id;
            var all = await ctx.Store.ListAsync(ctx.Store.Query<RemediationItem>().Where(i => i.SessionId == id).OrderBy(i => i.N), ctx.Ct);
            IEnumerable<RemediationItem> selected = all;
            var ids = itemIds is { Count: > 0 } ? itemIds.ToHashSet() : null;
            var groups = (groupKeys ?? []).Select(k => k.Split('|')).Where(p => p.Length >= 3).Select(p => (Ref: p[1].ToUpperInvariant(), Robt: p[2].ToUpperInvariant())).ToHashSet();
            if (ids is not null || groups.Count > 0)
                selected = selected.Where(i => ids?.Contains(i.Guid) == true || groups.Contains(((i.RefObject ?? string.Empty).ToUpperInvariant(), (i.RefType ?? string.Empty).ToUpperInvariant())));
            var items = selected.Where(filter).ToList();
            var systemId = ctx.SapSystem.Id;
            var caches = await ctx.Store.ListAsync(ctx.Store.Query<SourceCache>().Where(c => c.SapSystemId == systemId), ctx.Ct);
            return new StepData(session, items, caches);
        });
}
