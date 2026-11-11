namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>Parâmetros do run <c>Extraction</c>: <see cref="Force"/> extrai de novo uma execução já extraída (<c>-Forcar</c>).</summary>
public sealed record ExtractionRunParameters(bool Force);
