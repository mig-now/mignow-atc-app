namespace Mignow.Atc.Api.Contracts;

/// <summary>Corpo opcional de simular/enviar: só esses <c>seq</c> (motivos diferentes no mesmo lote = um run por motivo).</summary>
public sealed record ExemptionRunRequest(IReadOnlyList<int>? Seq = null);
