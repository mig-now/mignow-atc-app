using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// Porte da lógica de <c>inventario.ps1</c>: fora <c>$TMP</c>, marcados para exclusão (DELFLAG) e gerados (GENFLAG, salvo
/// <c>IncluirGerados</c>); ordem por tipo e nome; lotes por peso (heurística PROG/INTF 1, CLAS 3, FUGR 5): novo lote quando
/// o peso acumulado passaria do limite.
/// </summary>
public static class ScanInventoryBuilder
{
    public static readonly IReadOnlyList<string> DefaultTypes = ["PROG", "FUGR", "CLAS", "INTF"];

    private static readonly Dictionary<string, int> Weights = new(StringComparer.OrdinalIgnoreCase) { ["PROG"] = 1, ["INTF"] = 1, ["CLAS"] = 3, ["FUGR"] = 5 };

    public static int WeightOf(string type) => Weights.GetValueOrDefault(type);

    public static ScanInventory Build(IReadOnlyList<ScanTadirRow> rows, bool includeGenerated, int weightPerBatch, IReadOnlyList<string> types, string className)
    {
        int tmp = 0, deleted = 0, generated = 0;
        var scope = new List<ScanObject>();
        foreach (var r in rows)
        {
            if (string.Equals(r.Package, "$TMP", StringComparison.OrdinalIgnoreCase))
            {
                tmp++;
                continue;
            }
            if (r.DelFlag.Length > 0)
            {
                deleted++;
                continue;
            }
            if (r.GenFlag.Length > 0 && !includeGenerated)
            {
                generated++;
                continue;
            }
            scope.Add(new ScanObject(r.Type, r.Name, r.Package, r.Author));
        }
        // Sort-Object do PowerShell: comparação por cultura, sem diferenciar maiúsculas (estável).
        var ordered = scope.OrderBy(o => o.Type, StringComparer.InvariantCultureIgnoreCase).ThenBy(o => o.Name, StringComparer.InvariantCultureIgnoreCase).ToList();

        var batches = new List<(int Number, int Weight, List<ScanObject> Objects)>();
        foreach (var o in ordered)
        {
            var w = WeightOf(o.Type);
            if (batches.Count == 0 || batches[^1].Weight + w > weightPerBatch)
                batches.Add((batches.Count + 1, 0, []));
            var cur = batches[^1];
            batches[^1] = (cur.Number, cur.Weight + w, cur.Objects);
            cur.Objects.Add(o);
        }
        var byType = ordered.GroupBy(o => o.Type, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.InvariantCultureIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        return new ScanInventory([.. batches.Select(b => new ScanBatch(b.Number, b.Weight, b.Objects, className, null, false))], rows.Count, ordered.Count,
            byType, tmp, deleted, generated, includeGenerated, weightPerBatch, types);
    }
}
