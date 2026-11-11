using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Customers.Update;

public sealed class UpdateCustomerHandler(IDataStore store, IStringLocalizer<Errors> l)
    : ICommandHandler<UpdateCustomerCommand, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(UpdateCustomerCommand command, CancellationToken ct)
    {
        var customer = await store.FirstOrDefaultAsync(store.Query<Customer>().Where(c => c.Guid == command.Id), ct);
        if (customer is null)
            return l.NotFound("Customer.NotFound");
        var name = command.Name.Trim().ToLower();
        if (await store.AnyAsync(store.Read<Customer>().Where(c => c.Name.ToLower() == name && c.Id != customer.Id), ct))
            return l.Conflict("Customer.NameAlreadyExists", command.Name.Trim());
        customer.Update(command.Name, command.Code, command.Active);
        var systems = await store.CountAsync(store.Read<SapSystem>().Where(p => p.CustomerId == customer.Id), ct);
        return new CustomerResponse(customer.Guid, customer.Name, customer.Code, customer.Active, systems, customer.CreatedAt);
    }
}
