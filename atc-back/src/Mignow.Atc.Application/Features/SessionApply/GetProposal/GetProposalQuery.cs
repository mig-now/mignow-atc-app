using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.SessionApply.GetProposal;

public sealed record GetProposalQuery(Guid SessionId, Guid ItemId) : IQuery<Result<ProposalResponse>>;
