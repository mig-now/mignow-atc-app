namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>Entrada da revisão por IA antes de gravar (vai como JSON no prompt do usuário).</summary>
public sealed record ReviewTask(
    int N, string ObjectName, string ObjectType, string? Check, string? Message, string? Note, string Origin, string? KbCardId, string? Justification,
    IReadOnlyList<string> Alerts, bool? SyntaxOk, IReadOnlyList<string> SyntaxMessages, string Diff, IReadOnlyList<string> RelatedObjects);
