namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Resultado do agrupamento: pendentes, lote pedido, objetos e itens do lote e os grupos (cliente primeiro, depois standard; maiores primeiro).</summary>
public sealed record GroupingResult(int Pending, int Batch, int ObjectsInBatch, int ItemsInBatch, IReadOnlyList<RemediationGroup> Groups);
