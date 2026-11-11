namespace Mignow.Atc.Api.Contracts;

public sealed record ConfirmR1Request(IReadOnlyList<int>? ItemNs = null, bool Apply = true);
