using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Porte de <c>cmd_grupos</c>: o lote = objetos dos N primeiros pendentes da fila (todos os pendentes desses objetos entram), agrupados por
/// (standard?, objeto referenciado, RObT, forma de chamada). Cada grupo pergunta a KB (<see cref="IKnowledgeBase.LookupAsync"/>, top 1) e recebe
/// a complexidade e o modelo sugerido: sem fonte ou sem solução na KB = complexa (média se o uso não foi localizado); com solução, a do cartão.
/// </summary>
public static class GroupEngine
{
    public static IReadOnlyList<RemediationItem> BatchItems(IReadOnlyList<RemediationItem> items, int batch)
    {
        var pending = StandardObjects.PendingQueue(items);
        var objects = pending.Take(batch).Select(i => (i.ObjectType.ToUpperInvariant(), i.ObjectName.ToUpperInvariant())).ToHashSet();
        return [.. pending.Where(i => objects.Contains((i.ObjectType.ToUpperInvariant(), i.ObjectName.ToUpperInvariant())))];
    }

    public static async Task<GroupingResult> GroupAsync(IReadOnlyList<RemediationItem> items, int batch, SourceIndex sources, IKnowledgeBase kb, CancellationToken ct)
    {
        var pending = StandardObjects.PendingQueue(items);
        var lote = BatchItems(items, batch);
        var groups = new List<((bool Std, string Ref, string Robt, string Form) Key, List<RemediationItem> Items)>();
        var index = new Dictionary<(bool, string, string, string), int>();
        foreach (var it in lote)
        {
            var key = (StandardObjects.IsStandard(it.ObjectName), (it.RefObject ?? string.Empty).ToUpperInvariant(), (it.RefType ?? string.Empty).ToUpperInvariant(),
                sources.CallForm(it.ObjectName, it.RefObject ?? string.Empty));
            if (!index.TryGetValue(key, out var pos))
            {
                pos = groups.Count;
                index[key] = pos;
                groups.Add((key, []));
            }
            groups[pos].Items.Add(it);
        }
        var result = new List<RemediationGroup>();
        foreach (var (key, its) in groups.OrderBy(g => g.Key.Std).ThenByDescending(g => g.Items.Count))
        {
            KbCard? top = null;
            var total = 0;
            if (key.Ref.Length > 0)
            {
                var found = await kb.LookupAsync(key.Ref, key.Robt, 1, ct);
                top = found.Cards.FirstOrDefault();
                total = found.Total;
            }
            var complexity = key.Form is "sem_fonte" or "uso_nao_localizado" || top is null
                ? (key.Form != "uso_nao_localizado" ? "complexa" : "media")
                : top.Complexity;
            result.Add(new RemediationGroup(
                GroupKey(key.Std, key.Ref, key.Robt, key.Form), key.Ref, key.Robt, key.Form, [.. its.Select(i => i.N)],
                [.. its.Select(i => $"{i.ObjectType} {i.ObjectName}").Distinct().Order(StringComparer.Ordinal)],
                top is null ? null : new KbTop(top.Id, top.Occurrences, top.Alerts), total, complexity, key.Std, SuggestedModel(complexity)));
        }
        return new GroupingResult(pending.Count, batch, lote.Select(i => (i.ObjectType.ToUpperInvariant(), i.ObjectName.ToUpperInvariant())).Distinct().Count(),
            lote.Count, result);
    }

    public static string SuggestedModel(string complexity) => complexity switch { "simples" => "haiku", "media" => "sonnet", _ => "principal" };

    public static string GroupKey(bool standard, string reference, string refType, string callForm) =>
        $"{(standard ? "S" : "C")}|{reference}|{refType}|{callForm}";
}
