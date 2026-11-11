using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>Resultado do inventário (o <c>inventario.json</c> + <c>lotes.json</c> da skill).</summary>
public sealed record ScanInventory(
    IReadOnlyList<ScanBatch> Batches, int TadirRead, int ScopeCount, IReadOnlyDictionary<string, int> ByType, int ExcludedTmp, int ExcludedDeleted,
    int ExcludedGenerated, bool IncludeGenerated, int WeightPerBatch, IReadOnlyList<string> Types);
