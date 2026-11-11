namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>Resumo do play: itens escolhidos por situação, ativados no SAP, com erro de aplicação e standard deixados para o consultor.</summary>
public sealed record AutoRemediationSummary(int Items, int Kb, int Ai, int Exemption, int Cancelled, int Pending, int Activated, int ApplyErrors, int Standard,
    int AwaitingApproval = 0);
