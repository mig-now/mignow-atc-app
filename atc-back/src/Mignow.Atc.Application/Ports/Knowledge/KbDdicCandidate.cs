namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>BAPI/estrutura standard incluída nas soluções da KB (porte de <c>cmd_ddic_lista</c>): tipo (FUNC|STRU), nome, ocorrências e se está no cache.</summary>
public sealed record KbDdicCandidate(string Type, string Name, int Occurrences, bool Cached);
