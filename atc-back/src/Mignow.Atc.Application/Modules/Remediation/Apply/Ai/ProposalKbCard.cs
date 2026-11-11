namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>Cartão da KB (<c>kb.top</c>): código original e novo, alertas, complexidade e se foi substituído por uma contribuição.</summary>
public sealed record ProposalKbCard(
    string Id, int Occurrences, string Pattern, string Original, string New, IReadOnlyList<string> Alerts, string Complexity, string? ReplacedBy);
