namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Itens por situação (<c>contar</c> do resultado.py): KB, IA, Isenção, Cancelado, Pendente + objetos distintos e objetos com algum item pendente
/// (pontos do burndown por finding e por objeto). <c>ApplyErrors</c> = itens cuja última tentativa no SAP falhou (não entra no total: o item
/// continua na sua situação, ex. "Confirmado KB"). <c>AwaitingApproval</c> = pendentes de aprovação (proposta esperando a decisão do consultor) (parte de
/// <c>Pending</c>, não soma no total).
/// </summary>
public sealed record SituationCounts(int Kb, int Ai, int Exemption, int Cancelled, int Pending, int Objects, int PendingObjects, int ApplyErrors = 0,
    int AwaitingApproval = 0)
{
    public int Total => Kb + Ai + Exemption + Cancelled + Pending;
}
