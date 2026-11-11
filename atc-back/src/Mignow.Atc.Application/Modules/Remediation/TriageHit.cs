namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Item que a triagem classificou numa regra, com a observação exata que o script grava (<see cref="Operation"/> só na R8, <see cref="Calls"/> só na R5).</summary>
public sealed record TriageHit(int N, string ObjectName, string ObjectType, string Ref, string Obs, string? Operation = null, IReadOnlyList<string>? Calls = null);
