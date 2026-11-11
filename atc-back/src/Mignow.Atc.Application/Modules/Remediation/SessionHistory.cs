namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Histórico da sessão (parte de remediação do <c>resultado.py</c>, calculado do <c>StatusHistory</c> e dos Runs): situação antes (ao criar a
/// sessão) e depois (agora), transições (<c>"Pendente → KB"</c>), itens tratados, linha do tempo por dia, tempo de relógio e tempo líquido (sem
/// pausas acima de 10 min entre as atividades) e os Runs com tokens/paralelismo.
/// </summary>
public sealed record SessionHistory(
    SituationCounts Before, SituationCounts After, int Handled, IReadOnlyDictionary<string, int> Transitions, IReadOnlyList<int> HandledItems,
    IReadOnlyList<TimelinePoint> Timeline, int WallSeconds, int NetSeconds, IReadOnlyList<HistoryRun> Runs, int AiInputTokens, int AiOutputTokens,
    decimal AiCost, int AiCalls);
