namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>
/// Saída do <c>listar.py</c> para o consultor confirmar a lista: quantos itens e findings, de onde veio o vínculo
/// (<c>finding_id</c> = exato | <c>cruzamento</c> = sessões antigas), o que ficou de fora e o aviso <c>ProbablySemEscopo</c>
/// (checksum -1, ABAP Query: o SAP tende a não aceitar isenção só desse finding; a simulação confirma com SEM_ESCOPO).
/// </summary>
public sealed record ExemptionListingReport(
    int SelectedItems, int Findings, IReadOnlyList<string> Priorities, int OutOfScope, IReadOnlyList<ExemptionListingNote> WithoutPriority,
    IReadOnlyList<ExemptionDivergentGroup> DivergentGroups, IReadOnlyDictionary<string, int> ByOrigin, IReadOnlyList<ExemptionListingNote> WithoutLink,
    IReadOnlyList<ExemptionListingNote> AlreadyExempt, IReadOnlyList<ExemptionListingNote> AlreadyRequested,
    IReadOnlyList<ExemptionListingNote> ProbablyNoScope, IReadOnlyList<ExemptionListingEntry> Sample);
