using System.Text.Json;

namespace Mignow.Atc.Application.Dtos.Scans;

/// <summary>Varredura: escopo (contagens do inventário), lotes, regras ativas e resumo do cruzamento (como o <c>resumo.json</c>).</summary>
public sealed record ScanResponse(
    Guid Id, Guid SapSystemId, string SapSystemName, Guid? ExtractionId, JsonElement? Scope, IReadOnlyList<ScanBatchResponse> Batches,
    IReadOnlyList<string> ActiveRuleIds, int RuleCount, JsonElement? Summary, DateTimeOffset CreatedAt, string CreatedBy);
