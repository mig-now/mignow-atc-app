using Mignow.Atc.Application.Dtos.Extractions;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Resumo do run (<see cref="Domain.Execution.Run.ResultJson"/>), também a passagem de estado entre os passos.
/// <see cref="Situation"/>: <c>Checked</c> (só verificou), <c>Found</c> (achou a execução, ainda por extrair),
/// <c>AlreadyExtracted</c> (já extraída, nada a fazer) ou <c>Extracted</c>.
/// </summary>
public sealed record ExtractionRunResult(
    string Situation, AtcLatestResponse? Latest, Guid? ExtractionId = null, int? TotalCount = null, int? InBaselineCount = null, int? NewCount = null,
    PriorityCounts? Total = null, PriorityCounts? New = null, ExtractionVariation? Variation = null, ExtractionQuality? Quality = null, int? Columns = null)
{
    public const string Checked = "Checked";
    public const string Found = "Found";
    public const string AlreadyExtracted = "AlreadyExtracted";
    public const string Extracted = "Extracted";
}
