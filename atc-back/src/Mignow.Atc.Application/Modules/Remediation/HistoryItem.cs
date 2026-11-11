using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Visão mínima do item para o histórico: objeto, status atual e o <c>StatusHistory</c>.</summary>
public sealed record HistoryItem(int N, string ObjectKey, string Status, IReadOnlyList<StatusChange> History);
