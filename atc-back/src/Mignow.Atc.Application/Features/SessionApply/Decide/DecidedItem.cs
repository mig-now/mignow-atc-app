namespace Mignow.Atc.Application.Features.SessionApply.Decide;

public sealed record DecidedItem(Guid ItemId, int N, string Status, string Obs);
