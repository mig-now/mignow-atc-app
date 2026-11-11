namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>Motivos e validades do customizing do ATC (validade como <c>U = texto</c>).</summary>
public sealed record ExemptionCustomizing(IReadOnlyList<ExemptionReason> Reasons, IReadOnlyList<string> Validities);
