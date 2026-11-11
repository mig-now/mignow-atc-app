using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Customers.Delete;

public sealed class DeleteCustomerHandler(IDataStore store, IStringLocalizer<Errors> l) : ICommandHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerCommand command, CancellationToken ct)
    {
        var customer = await store.FirstOrDefaultAsync(store.Query<Customer>().Where(c => c.Guid == command.Id), ct);
        if (customer is null)
            return Result.Failure(l.NotFound("Customer.NotFound"));
        if (await store.AnyAsync(store.Read<SapSystem>().Where(p => p.CustomerId == customer.Id), ct))
            return Result.Failure(l.Conflict("Customer.HasSapSystems"));
        store.Remove(customer);
        return Result.Success();
    }
}
