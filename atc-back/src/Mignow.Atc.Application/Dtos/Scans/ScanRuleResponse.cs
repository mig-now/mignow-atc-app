namespace Mignow.Atc.Application.Dtos.Scans;

public sealed record ScanRuleResponse(
    string Id, bool Active, string Priority, string Category, string Severity, string Description, string OriginalPcre, string Pcre, string Text,
    string FixedElement, string Requires, string SuppressedBy, string Robt);
