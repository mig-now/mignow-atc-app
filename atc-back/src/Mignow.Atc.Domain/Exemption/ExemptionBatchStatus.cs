namespace Mignow.Atc.Domain.Exemption;

/// <summary>Etapa do lote no fluxo da skill atc-isencao (listar → descobrir → simular → enviar → conferir).</summary>
public enum ExemptionBatchStatus
{
    Created,
    Discovered,
    Simulated,
    Sent,
    Checked,
}
