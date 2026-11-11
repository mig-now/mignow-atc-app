namespace Mignow.Atc.Domain.Scan;

/// <summary>
/// Lote da varredura: nº, peso, objetos, classe temporária <c>ZCL_MIGNOW_ATC_SCAN_n</c> (a do processo que o executou),
/// status (<see cref="ScanBatchStatus"/>), se a classe foi apagada e as estatísticas da saída.
/// </summary>
public sealed record ScanBatch(int Number, int Weight, IReadOnlyList<ScanObject> Objects, string ClassName, string? Status, bool ClassRemoved,
    ScanBatchStats? Stats = null)
{
    public bool IsOk => Status == ScanBatchStatus.Ok;
}
