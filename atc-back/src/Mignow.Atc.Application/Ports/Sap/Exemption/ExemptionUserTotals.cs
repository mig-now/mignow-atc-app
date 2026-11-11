namespace Mignow.Atc.Application.Ports.Sap.Exemption;

/// <summary>Totais do usuário em <c>checkexemptionsview</c> (countTotal, abertos…). Atenção: já devolveu 0 mesmo com pedidos criados.</summary>
public sealed record ExemptionUserTotals(int Http, IReadOnlyDictionary<string, int> Totals);
