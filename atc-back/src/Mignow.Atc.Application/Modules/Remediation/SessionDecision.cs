namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Decisão do consultor sobre os itens (síncrona): grava só status/obs/histórico/ficha.</summary>
public enum SessionDecision
{
    Cancel,
    Exemption,
    ConfirmKb,
    ConfirmIa,
    Reset,

    /// <summary>Aceitar a sugestão da proposta de cada item: isenção, cancelamento ou confirmação da correção (KB/IA). "Sem solução" fica pendente.</summary>
    AcceptSuggestion,
}
