using System.Net;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>Resposta HTTP de erro da ADT (vira <c>Error.Upstream</c> no adapter; não sai dele).</summary>
internal sealed class AdtHttpException(string operation, HttpStatusCode status, string? sapMessage = null)
    : Exception($"ADT {operation}: HTTP {(int)status}")
{
    public HttpStatusCode Status { get; } = status;

    /// <summary>Texto do erro da ADT (exc:exception/message), para o log técnico. Nunca traz a credencial.</summary>
    public string? SapMessage { get; } = sapMessage;
}
