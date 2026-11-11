namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>Resposta da ADT fora do formato esperado (vira <c>Error.Upstream</c> no adapter).</summary>
internal sealed class AdtProtocolException(string message) : Exception(message);
