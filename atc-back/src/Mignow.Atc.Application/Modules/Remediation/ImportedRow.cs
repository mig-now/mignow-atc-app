namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Uma linha do CSV já interpretada (campos do item do <c>cmd_importar</c>) e a linha original completa.</summary>
public sealed record ImportedRow(
    int N, string ObjectName, string ObjectType, string Ref, string RefType, string Check, string Message, string Note, string Text, string Package,
    string Classification, string FindingId, string Priority, IReadOnlyList<string> RawRow);
