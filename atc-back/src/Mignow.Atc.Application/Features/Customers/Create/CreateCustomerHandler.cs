using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Customers.Create;

public sealed class CreateCustomerHandler(IDataStore store, IDateTimeProvider clock, IStringLocalizer<Errors> l)
    : ICommandHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        var name = command.Name.Trim().ToLower();
        if (await store.AnyAsync(store.Read<Customer>().Where(c => c.Name.ToLower() == name), ct))
            return l.Conflict("Customer.NameAlreadyExists", command.Name.Trim());
        var customer = Customer.Create(command.Name, command.Code);
        store.Add(customer);
        return new CustomerResponse(customer.Guid, customer.Name, customer.Code, customer.Active, 0, clock.UtcNow);
    }
}
