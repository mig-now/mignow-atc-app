namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Resultado da renderização: documento HTML completo e o fragmento (página principal de uma publicação).</summary>
public sealed record RenderedReport(string View, string Document, string Fragment, int Units, int BurndownPoints);
