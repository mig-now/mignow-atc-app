namespace Mignow.Atc.Api.Contracts;

public sealed record UpdateCustomerRequest(string Name, string? Code, bool Active);
