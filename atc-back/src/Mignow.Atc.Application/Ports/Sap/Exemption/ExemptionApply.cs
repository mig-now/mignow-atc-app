namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>Dados do pedido de um finding: o que cabe ao consultor (o resto vem do modelo do SAP). Escopo sempre só o finding.</summary>
public sealed record ExemptionApply(string MarkerId, string Approver, string Reason, string Justification, string Validity, string Notify = ExemptionApply.DefaultNotify)
{
    public const string DefaultNotify = "on_rejection";
}
