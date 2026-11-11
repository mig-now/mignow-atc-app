using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Implementação do <see cref="IDecisionHook"/> da parte 1 (<c>POST items/decision</c>): na <c>ConfirmIa</c> com proposta da IA registra UMA contribuição por
/// grupo (objeto referenciado + tipo) na KB — o primeiro item do pedido — com <c>replaces</c>/<c>reason</c>, e troca a observação pelo id do cartão
/// (<c>IA (C-…)</c>; nos demais do grupo <c>Grupo &lt;REF&gt;: IA (C-…)</c>), como o <c>registrar</c> da skill. Escopo da requisição: o dicionário vale por pedido.
/// </summary>
public sealed class KbContributionHook(IKnowledgeBase kb) : IDecisionHook
{
    private readonly Dictionary<string, string> _cards = [];

    public async Task<Result> OnDecidedAsync(DecisionContext context, CancellationToken ct)
    {
        var item = context.Item;
        if (context.Decision != SessionDecision.ConfirmIa || item.Proposal is not { Action: ItemProposal.ActionFix, Origin: ItemProposal.OriginAi } p || p.Blocks.Count == 0
            || string.IsNullOrWhiteSpace(item.RefObject))
            return Result.Success();
        var reference = item.RefObject.Trim();
        var key = $"{reference.ToUpperInvariant()}|{(item.RefType ?? string.Empty).ToUpperInvariant()}";
        string obs;
        if (_cards.TryGetValue(key, out var id))
            obs = $"Grupo {reference}: IA ({id})";
        else
        {
            var main = p.Blocks.Where(b => b.RelatedObject is null || b.RelatedObject == item.ObjectName.ToUpperInvariant()).ToList();
            id = await kb.RegisterAsync(new KbContribution(KbCategories.For(item.RefType), reference, item.Message,
                string.Join("\n...\n", main.Select(b => string.Join('\n', b.Before))), string.Join("\n...\n", main.Select(b => string.Join('\n', b.After))), context.Replaces, context.Reason), ct);
            _cards[key] = id;
            obs = string.IsNullOrWhiteSpace(context.Obs) ? $"IA ({id})" : context.Obs!;
        }
        item.MarkStatus(item.Status, obs, item.StatusDate ?? DateTimeOffset.UtcNow);
        return Result.Success();
    }
}
