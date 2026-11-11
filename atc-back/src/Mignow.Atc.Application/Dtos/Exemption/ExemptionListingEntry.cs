namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>Finding do lote (a "amostra" do <c>listar.py</c>): objeto, include e linha do <c>location</c> da extração.</summary>
public sealed record ExemptionListingEntry(int Seq, int ItemNumber, string ObjectName, string? Include, int? Line, string? RefObject, string MarkerId, string Origin);
