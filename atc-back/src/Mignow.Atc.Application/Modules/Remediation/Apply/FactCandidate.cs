namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Fato técnico sugerido pela IA para o item <see cref="N"/> (a gravação na KB é decisão do consultor: <c>facts</c> da decisão).</summary>
public sealed record FactCandidate(int N, string Element, string Fact, string? Proof, string? Impact);
