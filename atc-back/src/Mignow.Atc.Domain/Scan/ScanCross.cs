namespace Mignow.Atc.Domain.Scan;

/// <summary>Cruzamento da ocorrência com o CSV total do ATC (textos exatos do cruzar.py).</summary>
public static class ScanCross
{
    public const string AtcCaught = "ATC_PEGOU";
    public const string AtcOtherLine = "ATC_OUTRA_LINHA";
    public const string SourceWithFinding = "MIGNOW_FONTE_COM_FINDING";
    public const string SourceWithoutFinding = "MIGNOW_FONTE_SEM_FINDING";

    public static readonly IReadOnlyList<string> All = [AtcCaught, AtcOtherLine, SourceWithFinding, SourceWithoutFinding];
}
