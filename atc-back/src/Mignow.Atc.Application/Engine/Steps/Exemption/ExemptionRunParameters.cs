namespace Mignow.Atc.Application.Engine.Steps.Exemption;

/// <summary>Parâmetros das execuções do módulo (<c>Run.ParametersJson</c>): o lote e, opcionalmente, os <c>seq</c> a processar (todos quando nulo/vazio).</summary>
public sealed record ExemptionRunParameters(Guid BatchId, IReadOnlyList<int>? Seq = null);
