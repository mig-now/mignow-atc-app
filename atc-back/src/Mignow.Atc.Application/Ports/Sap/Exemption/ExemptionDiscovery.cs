namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>
/// Descoberta somente leitura do pedido de isenção (porte do <c>descobrir.ps1</c>): serviços ADT publicados, política,
/// motivos/validades, totais do usuário, aprovadores (<c>SATC_CI_APPROVER</c>/<c>SATC_CI_AREA</c>; nulo = tabela ilegível)
/// e, só informativo, serviços OData e funções RFC de isenção.
/// </summary>
public sealed record ExemptionDiscovery(
    bool ApplyPublished, IReadOnlyList<string> Collections, ExemptionPolicy Policy, ExemptionCustomizing Customizing, int? UserTotal,
    int? Approvers, int? Areas, IReadOnlyList<string> ODataServices, IReadOnlyList<string> RfcFunctions);
