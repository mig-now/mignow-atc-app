using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// Entrada do cruzamento (o que o <c>cruzar.py</c> lê de pastas): ocorrências na ordem de leitura (lote, linha da saída),
/// lotes (estatísticas), escopo (<c>inventario.json</c>), regras, findings do ATC e a identificação da base.
/// </summary>
public sealed record ScanCrossInput(
    IReadOnlyList<MignowOccurrence> Occurrences, IReadOnlyList<ScanBatch> Batches, ScanInventoryCounts Inventory, MignowRulesDocument Rules,
    IReadOnlyList<ScanAtcFinding> Findings, ScanAtcBase AtcBase, string ScanName, string Sid, string Client, string ClassName, bool IncludeOtherLine, DateTime Now);

/// <summary>Contagens do inventário (<c>inventario.json</c>): objetos no escopo, por tipo e excluídos.</summary>
public sealed record ScanInventoryCounts(int Scope, IReadOnlyDictionary<string, int> ByType, int ExcludedTmp, int ExcludedDeleted, int ExcludedGenerated);

/// <summary>Base ATC do cruzamento: identificação do CSV total da extração (arquivo, série, execução UTC).</summary>
public sealed record ScanAtcBase(string File, string? Series, string? ExecutedUtc);
