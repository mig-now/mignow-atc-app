namespace Mignow.Atc.Api.Contracts;

public sealed record ReuseRequest(Guid FromSessionId, IReadOnlyList<int>? ItemNs = null);
