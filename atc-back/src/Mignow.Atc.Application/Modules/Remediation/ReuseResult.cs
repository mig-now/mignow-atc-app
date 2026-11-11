namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Itens avaliados e a contagem por situação.</summary>
public sealed record ReuseResult(IReadOnlyDictionary<string, int> Summary, IReadOnlyList<ReuseCandidate> Items);
