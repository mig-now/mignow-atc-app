namespace Mignow.Atc.Domain.Extraction;

/// <summary>Resultado da extração: nova, ou a execução do ATC já tinha sido extraída (nunca extrai duas vezes, salvo force).</summary>
public enum ExtractionSituation
{
    Extracted,
    AlreadyExtracted,
}
