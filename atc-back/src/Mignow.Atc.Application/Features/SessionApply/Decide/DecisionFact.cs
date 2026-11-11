namespace Mignow.Atc.Application.Features.SessionApply.Decide;

/// <summary>Fato técnico a registrar na KB junto com a decisão (<c>atc.py fato</c>): elementos, o fato, a prova e o impacto/como usar.</summary>
public sealed record DecisionFact(IReadOnlyList<string> Elements, string Fact, string? Proof, string? Impact);
