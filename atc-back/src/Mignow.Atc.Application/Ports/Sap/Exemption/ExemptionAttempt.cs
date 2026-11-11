namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>
/// Resultado de simular ou enviar o pedido de um finding. <see cref="Status"/> usa os textos da skill
/// (<c>ExemptionRequestStatus</c>): na simulação SIMULADO | BLOQUEADO | SEM_ESCOPO; no envio PEDIDO | INFO | ERRO (ou
/// BLOQUEADO/SEM_ESCOPO se o modelo mudou). <see cref="RequestXml"/> é o corpo que seria/foi enviado; <see cref="Http"/>
/// e <see cref="SapType"/> (S|I|E) só existem depois do POST.
/// </summary>
public sealed record ExemptionAttempt(
    string Status, string? Message, string? ModelXml, string? RequestXml, string? ResponseXml, int? Http, string? SapType, string? ScopeReason);
