using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.List;

public sealed record ListSapSystemsQuery(Guid? CustomerId = null, bool? Active = null, string? Search = null, int Page = 1, int PageSize = 20)
    : IQuery<Result<PagedResult<SapSystemResponse>>>;
