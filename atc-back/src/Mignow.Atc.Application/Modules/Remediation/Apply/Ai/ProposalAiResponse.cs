namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>Saída da IA: uma proposta por item da tarefa (nenhum fica de fora).</summary>
public sealed record ProposalAiResponse(IReadOnlyList<ProposalAiItem> Items);
