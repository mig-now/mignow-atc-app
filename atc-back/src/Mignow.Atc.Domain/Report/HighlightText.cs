namespace Mignow.Atc.Domain.Report;

/// <summary>Destaque manual do consultor (texto livre com a fonte), do <c>destaques</c> do <c>complementos.json</c>.</summary>
public sealed record HighlightText(string Texto, string? Fonte);
