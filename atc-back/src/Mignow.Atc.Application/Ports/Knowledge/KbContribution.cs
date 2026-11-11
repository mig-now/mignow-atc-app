namespace Mignow.Atc.Application.Ports.Knowledge;

/// <summary>
/// Contribuição <c>Confirmado IA</c> (porte de <c>cmd_registrar</c>): vira cartão <c>C-&lt;ELEM&gt;-NNN</c> em
/// <c>contribuicoes/&lt;categoria&gt;/&lt;ELEM&gt;.md</c>. <see cref="Replaces"/> = ids substituídos (com <see cref="Reason"/>).
/// </summary>
public sealed record KbContribution(string Category, string Element, string? Problem, string Before, string After, IReadOnlyList<string> Replaces,
    string? Reason);
