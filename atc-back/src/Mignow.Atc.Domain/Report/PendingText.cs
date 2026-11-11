namespace Mignow.Atc.Domain.Report;

/// <summary>Pendência manual do consultor (ex.: teste funcional), do <c>pendencias</c> do <c>complementos.json</c>.</summary>
public sealed record PendingText(string Titulo, string Texto, string? Fonte);
