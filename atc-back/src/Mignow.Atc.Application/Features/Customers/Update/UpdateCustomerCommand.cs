using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.Customers.Update;

public sealed record UpdateCustomerCommand(Guid Id, string Name, string? Code, bool Active) : ICommand<Result<CustomerResponse>>;
