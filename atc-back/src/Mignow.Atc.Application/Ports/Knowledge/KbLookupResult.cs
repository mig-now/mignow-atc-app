namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Os <c>top</c> cartões ordenados como o <c>kb_lookup</c> e o total encontrado.</summary>
public sealed record KbLookupResult(IReadOnlyList<KbCard> Cards, int Total);
