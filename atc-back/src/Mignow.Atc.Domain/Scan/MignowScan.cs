using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Scan;

/// <summary>Varredura MIGNOW (skill atc-extracao-mignow) de um ambiente, cruzada com o CSV total de uma extração.</summary>
public sealed class MignowScan : EntityBase
{
    private MignowScan() { }

    public int SapSystemId { get; private set; }

    /// <summary>Extração (CSV total) usada no cruzamento.</summary>
    public int? ExtractionId { get; private set; }

    /// <summary>Regras (<c>regras.json</c>) no momento da varredura.</summary>
    public string RulesSnapshotJson { get; private set; } = "[]";

    /// <summary>Contagens do inventário/escopo (objetos por tipo, pesos).</summary>
    public string? ScopeCountsJson { get; private set; }

    public IReadOnlyList<ScanBatch> Batches { get; private set; } = [];

    /// <summary>Resumo (como o <c>resumo.json</c> da skill).</summary>
    public string? SummaryJson { get; private set; }

    public static MignowScan Create(int sapSystemId, int? extractionId, string rulesSnapshotJson)
    {
        if (sapSystemId <= 0)
            throw new DomainException("Scan.SapSystemRequired", "A varredura precisa do ambiente.");
        return new MignowScan
        {
            SapSystemId = sapSystemId,
            ExtractionId = extractionId,
            RulesSnapshotJson = string.IsNullOrWhiteSpace(rulesSnapshotJson) ? "[]" : rulesSnapshotJson,
        };
    }

    public void SetScope(string? scopeCountsJson) => ScopeCountsJson = scopeCountsJson;

    /// <summary>Troca o lote de mesmo número (a lista é recriada: o jsonb detecta a alteração).</summary>
    public void ReplaceBatch(ScanBatch batch) => Batches = [.. Batches.Select(b => b.Number == batch.Number ? batch : b)];

    public void SetBatches(IEnumerable<ScanBatch> batches) => Batches = [.. batches.OrderBy(b => b.Number)];

    public void SetSummary(string? summaryJson) => SummaryJson = summaryJson;

    public void SetRules(string rulesSnapshotJson) =>
        RulesSnapshotJson = string.IsNullOrWhiteSpace(rulesSnapshotJson) ? "[]" : rulesSnapshotJson;

    public void SetExtraction(int? extractionId) => ExtractionId = extractionId;
}
