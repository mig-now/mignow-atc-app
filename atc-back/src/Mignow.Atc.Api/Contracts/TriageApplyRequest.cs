namespace Mignow.Atc.Api.Contracts;

public sealed record TriageApplyRequest(IReadOnlyList<string> Rules);
