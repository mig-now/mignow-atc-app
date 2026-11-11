namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>Fim de um processo: se a classe foi criada, se rodou sem erro de comunicação (<see cref="Error"/> nulo) e a remoção.</summary>
public sealed record ScanProcessResult(string ClassName, bool Created, string? Error, ScanClassRemoval Removal);
