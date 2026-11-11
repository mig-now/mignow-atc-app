namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>Pedido de um finding: item de origem, markerId, motivo/justificativa, status, mensagem do SAP e os XML enviado/recebido.</summary>
public sealed record ExemptionRequestResponse(
    Guid Id, int Seq, int ItemNumber, string ObjectName, string ObjectType, string? RefObject, string? Check, string? Message, string MarkerId, string Reason,
    string Justification, string? Status, bool ProbablyNoScope, string? SapMessage, string? RequestXml, string? ResponseXml);
