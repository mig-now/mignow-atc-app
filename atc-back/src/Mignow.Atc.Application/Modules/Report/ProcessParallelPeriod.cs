namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Execução que usou paralelismo (&gt; 1): período e grau máximo configurado.</summary>
public sealed record ProcessParallelPeriod(DateTimeOffset Start, DateTimeOffset End, int Parallelism, string Kind);
