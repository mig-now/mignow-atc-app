namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Tudo de um elemento: cartões da base, contribuições, fatos e substituições.</summary>
public sealed record KbElementDetail(string Category, string Element, IReadOnlyList<KbCard> Cards, IReadOnlyList<KbCard> Contributions,
    IReadOnlyList<KbFact> Facts, IReadOnlyList<KbReplacement> Replacements);
