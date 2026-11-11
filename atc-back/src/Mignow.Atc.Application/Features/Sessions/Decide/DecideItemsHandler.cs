using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Decide;

/// <summary>
/// Grava status/obs/histórico (<c>MarkStatus</c> = <c>marcar()</c>) e, nas confirmações com proposta, a ficha da correção (<c>cmd_status</c>).
/// Depois de cada item chama os <see cref="IDecisionHook"/> (parte 2: KB, vínculo à proposta); falha de um hook desfaz tudo (sem commit).
/// </summary>
public sealed class DecideItemsHandler(IDataStore store, IDateTimeProvider clock, IEnumerable<IDecisionHook> hooks, IStringLocalizer<Errors> l)
    : ICommandHandler<DecideItemsCommand, Result<DecideItemsResponse>>
{
    public static string StatusOf(SessionDecision d) => d switch
    {
        SessionDecision.Cancel => RemediationStatus.Cancelled,
        SessionDecision.Exemption => RemediationStatus.Exemption,
        SessionDecision.ConfirmKb => RemediationStatus.ConfirmedKb,
        SessionDecision.ConfirmIa => RemediationStatus.ConfirmedAi,
        _ => RemediationStatus.Pending,
    };

    public async Task<Result<DecideItemsResponse>> Handle(DecideItemsCommand command, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, command.SessionId, true, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        if (session.Archived)
            return l.Conflict("Session.Archived");
        var sid = session.Id;
        var ids = command.ItemIds.Distinct().ToArray();
        var items = await store.ListAsync(store.Query<RemediationItem>().Where(i => i.SessionId == sid && ids.Contains(i.Guid)), ct);
        if (items.Count != ids.Length)
            return l.NotFound("Session.ItemNotFound");
        var now = clock.UtcNow;
        var delta = 0;
        var skipped = 0;
        foreach (var item in items.OrderBy(i => i.N))
        {
            // "aceitar a sugestão": cada item recebe a decisão que a própria proposta recomenda (observação = a do consultor ou a justificativa)
            var decision = command.Decision == SessionDecision.AcceptSuggestion ? SuggestionOf(item) : command.Decision;
            if (decision is null)
            {
                skipped++;
                continue;
            }
            var status = StatusOf(decision.Value);
            var obs = command.Decision == SessionDecision.AcceptSuggestion && string.IsNullOrWhiteSpace(command.Obs)
                ? Truncate(item.Proposal?.Justification ?? string.Empty, 4000)
                : command.Obs ?? string.Empty;
            var previous = item.Status;
            delta += (status.Length == 0 ? 1 : 0) - (previous.Length == 0 ? 1 : 0);
            item.MarkStatus(status, obs, now);
            if (decision is SessionDecision.ConfirmKb or SessionDecision.ConfirmIa && CorrectionSheet.FromProposal(item.Proposal) is { } code)
                item.SetCorrectionSheet(CorrectionSheet.Render(item, status, obs, code.Before, code.After, item.CorrectionSheetMd));
            foreach (var hook in hooks)
            {
                var result = await hook.OnDecidedAsync(new DecisionContext(session, item, decision.Value, previous, obs, command.Replaces ?? [], command.Reason), ct);
                if (result.IsFailure)
                    return result.Error;
            }
        }
        var pending = await store.CountAsync(store.Read<RemediationItem>().Where(i => i.SessionId == sid && i.Status == RemediationStatus.Pending), ct);
        pending += delta; // a contagem do banco ainda é a anterior (o commit vem depois)
        return new DecideItemsResponse([.. items.OrderBy(i => i.N).Select(i => i.ToResponse())], pending, skipped);
    }

    /// <summary>A decisão que a proposta pendente recomenda; <c>null</c> = nada a aceitar (sem proposta, sem solução ou item já decidido).</summary>
    public static SessionDecision? SuggestionOf(RemediationItem item) =>
        item.Status != RemediationStatus.Pending ? null : item.Proposal switch
        {
            { Action: ItemProposal.ActionExemption } => SessionDecision.Exemption,
            { Action: ItemProposal.ActionCancel } => SessionDecision.Cancel,
            { Action: ItemProposal.ActionFix, Origin: ItemProposal.OriginKb } => SessionDecision.ConfirmKb,
            { Action: ItemProposal.ActionFix } => SessionDecision.ConfirmIa,
            _ => null,
        };

    private static string Truncate(string text, int max) => text.Length <= max ? text : text[..max];
}
