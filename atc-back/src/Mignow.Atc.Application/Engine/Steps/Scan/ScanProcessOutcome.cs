namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Fim dos processos de um passo: lotes com erro (<c>lote:status</c>), erros de processo, classes não removidas e se o consultor interrompeu.</summary>
public sealed record ScanProcessOutcome(
    IReadOnlyList<string> FailedLots, IReadOnlyList<string> ProcessErrors, IReadOnlyList<string> ClassesNotRemoved, bool Cancelled, IReadOnlyList<int> LotsDone)
{
    public bool HasFailure => FailedLots.Count > 0 || ProcessErrors.Count > 0;
}
