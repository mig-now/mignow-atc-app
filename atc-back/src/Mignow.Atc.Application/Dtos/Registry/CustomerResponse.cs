namespace Mignow.Atc.Application.Dtos.Registry;

public sealed record CustomerResponse(Guid Id, string Name, string? Code, bool Active, int SapSystemCount, DateTimeOffset CreatedAt);
