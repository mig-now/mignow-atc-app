namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>DDIC lido do sistema (somente leitura) antes de propor: estrutura, tabela, assinatura de função ou elemento de dados.</summary>
public sealed record ProposalDdic(string Name, string Kind, string Source);
