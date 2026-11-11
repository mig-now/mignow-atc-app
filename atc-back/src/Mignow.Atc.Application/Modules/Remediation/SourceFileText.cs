namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Um fonte lido do SAP (uma linha do <c>SourceCache</c>): objeto, nome do arquivo sem extensão (include, <c>.definitions</c>…) e texto.</summary>
public sealed record SourceFileText(string ObjectName, string Name, string Text);
