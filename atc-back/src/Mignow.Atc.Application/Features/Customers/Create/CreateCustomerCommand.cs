using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.Customers.Create;

public sealed record CreateCustomerCommand(string Name, string? Code) : ICommand<Result<CustomerResponse>>;
