using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Customers.Delete;

/// <summary>Apaga o cliente (só sem ambientes).</summary>
public sealed record DeleteCustomerCommand(Guid Id) : ICommand<Result>;
