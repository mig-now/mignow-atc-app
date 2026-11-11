namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>Finding que entrou no lote (a <c>lista.json</c> do <c>listar.py</c>): o item que o representa, o vínculo e o aviso de escopo.</summary>
public sealed record ExemptionListingEntryData(
    int Seq, ExemptionListingItem Item, IReadOnlyList<int> GroupItemNumbers, string MarkerId, string Origin, string? Include, int? Line, string ScopeWarning);
