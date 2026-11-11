using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.Customers.List;

public sealed record ListCustomersQuery(int Page = 1, int PageSize = 20, string? Search = null, bool? Active = null)
    : IQuery<Result<PagedResult<CustomerResponse>>>;
