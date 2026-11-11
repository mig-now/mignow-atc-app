namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Resultado de <c>cmd_ddic_lista</c>: candidatos (mais frequentes) e os que faltam no cache DDIC.</summary>
public sealed record KbDdicCandidates(IReadOnlyList<KbDdicCandidate> Candidates, IReadOnlyList<string> MissingFromCache);
