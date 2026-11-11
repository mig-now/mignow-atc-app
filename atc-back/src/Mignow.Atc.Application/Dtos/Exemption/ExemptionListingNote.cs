namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>Item ou finding que saiu do lote (ou merece atenção), com o motivo: <c>Detail</c> = prioridade, aprovação, aviso de escopo…</summary>
public sealed record ExemptionListingNote(int ItemNumber, string? ObjectName, string? MarkerId, string? Detail);
