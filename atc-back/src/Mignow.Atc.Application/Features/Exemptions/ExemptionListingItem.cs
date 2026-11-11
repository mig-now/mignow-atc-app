namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>Item da sessão como o <c>listar.py</c> o lê (<c>objeto</c>, <c>tipo</c>, <c>ref</c>, <c>msg</c>, <c>check</c>, <c>obs</c>, <c>prioridade</c>, <c>finding_id</c>).</summary>
public sealed record ExemptionListingItem(
    int Id, int N, string ObjectName, string ObjectType, string? RefObject, string? Check, string? Message, string? Obs, string? Status, string? Priority,
    string? FindingId);
