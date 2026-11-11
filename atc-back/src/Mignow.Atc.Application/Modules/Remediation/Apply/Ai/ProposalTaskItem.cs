namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>Item do ATC na tarefa: número da linha do CSV, linha do uso no fonte (se conhecida) e o finding.</summary>
public sealed record ProposalTaskItem(int N, int? Line, string? Check, string? Message, string? Note, string? Classification, string? Text);
