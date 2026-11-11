using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Knowledge;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.SessionApply.Decide;

/// <summary>
/// Porte do passo 7 da Etapa 4 (<c>status</c> + <c>registrar</c> + <c>fato</c>): grava Confirmado KB/IA com a ficha; no ConfirmIa registra UMA contribuição por
/// grupo (a do primeiro item) e os demais ficam com <c>Grupo &lt;REF&gt;: IA (C-…)</c>. A KB é gravada uma por vez; o banco, pelo decorator de transação.
/// </summary>
public sealed class DecideProposalHandler(IDataStore store, IKnowledgeBase kb, IDateTimeProvider clock, IStringLocalizer<Errors> l)
    : ICommandHandler<DecideProposalCommand, Result<DecideProposalResponse>>
{
    public async Task<Result<DecideProposalResponse>> Handle(DecideProposalCommand c, CancellationToken ct)
    {
        var session = await SessionApplyLoader.SessionAsync(store, c.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var items = await SessionApplyLoader.ItemsAsync(store, session.Id, c.ItemIds, true, ct);
        if (items.Count != c.ItemIds.Distinct().Count())
            return l.NotFound("Item.NotFound");
        var ia = c.Decision == "ConfirmIa";
        foreach (var i in items)
        {
            if (i.Proposal is not { Action: ItemProposal.ActionFix } p || p.Blocks.Count == 0 || i.Preparation?.NewSource is null)
                return l.Conflict("Apply.Decision.NoProposal", i.N);
            if (i.Application?.Result is ApplyResults.Activated)
                return l.Conflict("Apply.Decision.AlreadyApplied", i.N);
            if (!ia && p.Origin != ItemProposal.OriginKb)
                return l.Conflict("Apply.Decision.NotKb", i.N);
            if (ia && p.Origin != ItemProposal.OriginAi)
                return l.Conflict("Apply.Decision.NotAi", i.N);
        }
        var now = clock.UtcNow;
        string? cardId = null;
        var decided = new List<DecidedItem>();
        Dictionary<string, string> groupCard = [];
        foreach (var i in items)
        {
            string obs;
            if (!ia)
                obs = string.IsNullOrWhiteSpace(c.Obs) ? $"KB {i.Proposal!.KbCardId}" : c.Obs!;
            else
            {
                var reference = (i.RefObject ?? string.Empty).Trim();
                if (reference.Length == 0)
                    return l.Conflict("Apply.Decision.NoReference", i.N);
                var key = $"{reference.ToUpperInvariant()}|{(i.RefType ?? string.Empty).ToUpperInvariant()}";
                if (!groupCard.TryGetValue(key, out var id))
                {
                    var main = i.Proposal!.Blocks.Where(b => b.RelatedObject is null || b.RelatedObject == i.ObjectName.ToUpperInvariant()).ToList();
                    id = await kb.RegisterAsync(new KbContribution(KbCategories.For(i.RefType), reference, i.Message, Join(main.Select(b => b.Before)), Join(main.Select(b => b.After)),
                        c.Replaces ?? [], c.Reason), ct);
                    groupCard[key] = id;
                    cardId ??= id;
                    obs = string.IsNullOrWhiteSpace(c.Obs) ? $"IA ({id})" : c.Obs!;
                }
                else
                    obs = $"Grupo {reference}: IA ({id})";
            }
            var status = ia ? RemediationStatus.ConfirmedAi : RemediationStatus.ConfirmedKb;
            i.MarkStatus(status, obs, now);
            if (CorrectionSheet.FromProposal(i.Proposal) is { } sheet)
                i.SetCorrectionSheet(CorrectionSheet.Render(i, status, obs, sheet.Before, sheet.After, i.CorrectionSheetMd));
            decided.Add(new DecidedItem(i.Guid, i.N, status, obs));
        }
        List<string> factIds = [];
        foreach (var f in c.Facts ?? [])
            factIds.AddRange(await kb.AddFactAsync(new KbFactInput(f.Elements, f.Fact, f.Proof, f.Impact), ct));
        return new DecideProposalResponse(decided, cardId, factIds);
    }

    /// <summary>Blocos separados por <c>...</c> (como os trechos da KB), código sem marcadores.</summary>
    private static string Join(IEnumerable<IReadOnlyList<string>> blocks) => string.Join("\n...\n", blocks.Select(b => string.Join('\n', b)));
}
