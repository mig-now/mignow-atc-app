namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>Itens do mesmo vínculo (cruzamento) cuja quantidade difere da de findings na extração (<c>grupos_itens_x_findings_divergentes</c>).</summary>
public sealed record ExemptionDivergentGroup(IReadOnlyList<int> ItemNumbers, string ObjectName, string? RefObject, int Findings);
