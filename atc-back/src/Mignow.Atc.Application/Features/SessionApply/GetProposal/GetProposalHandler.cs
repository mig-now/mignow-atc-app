using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation.Apply.Ai;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.SessionApply.GetProposal;

public sealed class GetProposalHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetProposalQuery, Result<ProposalResponse>>
{
    public async Task<Result<ProposalResponse>> Handle(GetProposalQuery q, CancellationToken ct)
    {
        var session = await SessionApplyLoader.SessionAsync(store, q.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var item = (await SessionApplyLoader.ItemsAsync(store, session.Id, [q.ItemId], false, ct)).FirstOrDefault();
        if (item is null)
            return l.NotFound("Item.NotFound");
        var p = item.Proposal;
        return new ProposalResponse(item.Guid, item.N, item.Status, p, item.Preparation?.Diff ?? p?.Diff, p?.Review, ReviewPrompts.VerdictOf(p?.Review), p?.Alerts ?? [], p?.SyntaxOk,
            item.Preparation, item.Application, p?.FactCandidates ?? []);
    }
}
