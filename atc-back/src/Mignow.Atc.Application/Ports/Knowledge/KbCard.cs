namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>
/// Cartão da KB (porte de <c>parse_cards</c> + enriquecimento do <c>kb_lookup</c>): id (<c>…-NNN</c> ou <c>C-&lt;ELEM&gt;-NNN</c>),
/// ocorrências, padrão, código original e novo, arquivo de origem, se é contribuição, alertas, complexidade
/// (simples|media|complexa) e por quem foi substituído.
/// </summary>
public sealed record KbCard(string Id, int Occurrences, string Pattern, string Original, string New, string KbSource, bool IsContribution,
    IReadOnlyList<string> Alerts, string Complexity, string? ReplacedBy);
