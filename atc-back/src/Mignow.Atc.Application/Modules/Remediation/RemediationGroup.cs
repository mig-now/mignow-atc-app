namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Grupo de itens do mesmo objeto referenciado e da mesma forma de chamada (<c>cmd_grupos</c>). <see cref="Key"/> identifica o grupo
/// (<c>C|S</c> + ref + robt + forma). <see cref="Complexity"/> = simples|media|complexa; <see cref="SuggestedModel"/> = haiku|sonnet|principal
/// (do script; o motor traduz para o uso de IA Mechanical|Review|Analysis).
/// </summary>
public sealed record RemediationGroup(
    string Key, string Ref, string RefType, string CallForm, IReadOnlyList<int> Items, IReadOnlyList<string> Objects, KbTop? KbTop,
    int KbSolutions, string Complexity, bool Standard, string SuggestedModel);
