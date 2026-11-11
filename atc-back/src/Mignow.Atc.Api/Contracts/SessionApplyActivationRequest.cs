namespace Mignow.Atc.Api.Contracts;

/// <summary>Corpo de <c>POST sessions/{id}/items/{itemId}/activation-decision</c>: <c>keepInactive</c> | <c>rollback</c>.</summary>
public sealed record SessionApplyActivationRequest(string Decision);
