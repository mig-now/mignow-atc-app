namespace Mignow.Atc.Domain.Scan;

/// <summary>Situação da ocorrência (flag da saída da classe): P pendente, R já remediada (bloco MIG-NOW), I informativa.</summary>
public static class ScanOccurrenceStatus
{
    public const string Pending = "P";
    public const string Remediated = "R";
    public const string Informative = "I";
}
