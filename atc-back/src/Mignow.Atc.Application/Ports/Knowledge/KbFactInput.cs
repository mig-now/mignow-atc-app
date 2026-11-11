namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Novo fato (porte de <c>cmd_fato</c>): um ou mais elementos, o fato, a prova e o impacto/como usar.</summary>
public sealed record KbFactInput(IReadOnlyList<string> Elements, string Fact, string? Proof, string? Impact);
