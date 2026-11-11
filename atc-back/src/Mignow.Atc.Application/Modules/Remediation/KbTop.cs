namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Melhor solução da KB do grupo (<c>kb_top</c> do <c>cmd_grupos</c>).</summary>
public sealed record KbTop(string Id, int Occurrences, IReadOnlyList<string> Alerts);
