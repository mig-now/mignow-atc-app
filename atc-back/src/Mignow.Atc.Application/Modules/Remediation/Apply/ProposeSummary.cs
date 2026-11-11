namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Resumo da proposta (vai para o <c>ResultJson</c> do run): contagens por ação/origem e os fatos candidatos à KB.</summary>
public sealed record ProposeSummary(
    int Items, int Fix, int Exemption, int Cancel, int Pending, int FromKb, int FromAi, int WithAlerts, int AiFailures, IReadOnlyList<FactCandidate> FactCandidates);
