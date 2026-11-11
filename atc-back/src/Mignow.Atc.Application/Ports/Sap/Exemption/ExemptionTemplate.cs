namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>
/// Modelo do pedido que o SAP devolve para um finding (<c>GET /sap/bc/adt/atc/exemptions/apply?markerId=</c>). Sem
/// <see cref="HasProposal"/> o SAP não deixa pedir e <see cref="Message"/> diz por quê. <see cref="ThisFinding"/> = o modelo
/// permite o escopo só deste finding (<c>restriction/thisFinding enabled</c>).
/// </summary>
public sealed record ExemptionTemplate(
    int Http, bool HasProposal, string Message, string Type, bool ThisFinding, string Checksum, string ObjectType, string ScopeReason, string Xml);
