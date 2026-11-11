namespace Mignow.Atc.Domain.Execution;

/// <summary>Tipo de execução: um pipeline cada (desenho §3.7).</summary>
public enum RunKind
{
    ConnectionTest,
    ExtractionCheck,
    Extraction,
    ReadSources,
    ConfirmR1,
    Propose,
    ApplySimulate,
    Apply,
    Rollback,
    ExemptionDiscover,
    ExemptionSimulate,
    ExemptionSend,
    ExemptionCheck,
    ScanInventory,
    ScanSimulate,
    ScanExecute,
    ScanCleanup,
    /// <summary>Remediação automática da sessão ("play"): fontes → triagem → R1 → proposta KB/IA → confirmação → simulação → revisão → aplicação.</summary>
    AutoRemediation,
}
