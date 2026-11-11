using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Dtos.Extractions;

/// <summary>
/// Linha do monitor de extrações: um ambiente ativo, a última execução de extração (qualquer situação, com passos) e a
/// última extração gravada. Base para disparar e acompanhar a extração de vários ambientes ao mesmo tempo.
/// </summary>
public sealed record ExtractionMonitorRow(
    Guid SapSystemId, string SapSystemName, Guid CustomerId, string CustomerName, bool IsProduction, bool IsSimulator, RunResponse? LastRun,
    ExtractionResponse? LastExtraction);
