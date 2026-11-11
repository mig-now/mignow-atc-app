namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>Janela do fonte (sem numeração: o texto é o do fonte, para o <c>antes</c> sair exato); <see cref="FromLine"/> = linha da primeira linha.</summary>
public sealed record ProposalWindow(int FromLine, string Text);
