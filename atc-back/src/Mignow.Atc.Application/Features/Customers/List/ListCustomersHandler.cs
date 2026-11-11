using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Customers.List;

public sealed class ListCustomersHandler(IDataStore store) : IQueryHandler<ListCustomersQuery, Result<PagedResult<CustomerResponse>>>
{
    public async Task<Result<PagedResult<CustomerResponse>>> Handle(ListCustomersQuery query, CancellationToken ct)
    {
        var q = store.Read<Customer>();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(c => c.Name.ToLower().Contains(s) || (c.Code != null && c.Code.ToLower().Contains(s)));
        }
        if (query.Active is not null)
            q = q.Where(c => c.Active == query.Active);
        var projected = q.OrderBy(c => c.Name).Select(c => new CustomerResponse(
            c.Guid, c.Name, c.Code, c.Active, store.Read<SapSystem>().Count(p => p.CustomerId == c.Id), c.CreatedAt));
        return await store.PageAsync(projected, query.Page, query.PageSize, ct);
    }
}
