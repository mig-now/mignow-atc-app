namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Entrada de <c>contribuicoes/substituicoes.json</c>: cartão substituído, por qual, quando e por quê.</summary>
public sealed record KbReplacement(string CardId, string ReplacedBy, string Element, string Date, string? Reason);
