namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>Fato técnico <c>F-&lt;ELEM&gt;-NNN</c> (<c>contribuicoes/fatos/&lt;ELEM&gt;.md</c>).</summary>
public sealed record KbFact(string Id, string Date, string Text);
