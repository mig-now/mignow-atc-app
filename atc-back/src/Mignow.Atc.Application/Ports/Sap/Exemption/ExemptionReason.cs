namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>Motivo de isenção do customizing do ATC (<c>/sap/bc/adt/atc/customizing</c>, <c>&lt;reason&gt;</c>).</summary>
public sealed record ExemptionReason(string Id, bool JustificationMandatory, string Title);
