namespace Mignow.Atc.Api.Contracts;

/// <summary>Leitura de fontes: lote (padrão = BatchSize), incluir objetos standard (confirmado pelo consultor) e reler o que já está em cache.</summary>
public sealed record ReadSourcesRequest(int? Batch = null, bool IncludeStandard = false, bool Force = false);
