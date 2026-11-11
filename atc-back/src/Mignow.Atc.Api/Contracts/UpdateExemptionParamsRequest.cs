namespace Mignow.Atc.Api.Contracts;

public sealed record UpdateExemptionParamsRequest(string? Approver, string Reason, string Validity, string? JustificationPrefix = null);
