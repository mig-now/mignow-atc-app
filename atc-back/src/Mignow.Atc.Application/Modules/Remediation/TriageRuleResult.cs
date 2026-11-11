namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Itens de uma regra. <see cref="Effect"/> = o status que a regra grava ao aplicar, ou <c>signal</c> (R3/R4/R6: só sinalizam).
/// <see cref="Key"/> = identidade fixa da regra (R1..R9, produto ou a da regra nova); <see cref="Code"/> = código exibido (renomeável).
/// </summary>
public sealed record TriageRuleResult(string Code, string Description, string Effect, IReadOnlyList<TriageHit> Items, string? Key = null)
{
    public const string Signal = "signal";

    public bool IsSignalOnly => Effect == Signal;
}
