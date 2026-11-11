using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.List;

public sealed record ListSessionsQuery(Guid? SapSystemId, Guid? CustomerId, bool? Archived, int Page = 1, int PageSize = 20)
    : IQuery<Result<PagedResult<SessionResponse>>>;
