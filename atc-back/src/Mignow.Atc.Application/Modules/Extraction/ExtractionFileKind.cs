namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>Os três CSVs da extração: total, só os sem baseline (<c>_novos</c>) e o layout da remediação (<c>_remediacao</c>).</summary>
public enum ExtractionFileKind
{
    Total,
    New,
    Remediation,
}
