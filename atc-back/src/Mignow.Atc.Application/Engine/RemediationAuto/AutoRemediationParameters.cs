namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>
/// Parâmetros do run <c>AutoRemediation</c> ("play" da sessão). Os itens escolhidos (<see cref="ItemIds"/>; vazio = todos os pendentes) passam por
/// todo o fluxo sem parada. As demais propriedades existem porque o pipeline reaproveita passos de outros runs, que leem os próprios parâmetros
/// do mesmo JSON: leitura de fontes (<see cref="Force"/>), conferência da R1 (<see cref="ItemNs"/>, <see cref="Apply"/>) e proposta/aplicação
/// automáticas (<see cref="GroupKeys"/>, <see cref="Mode"/>, <see cref="AutoConfirm"/>). Use <see cref="For"/>.
/// </summary>
public sealed record AutoRemediationParameters(
    Guid SessionId, IReadOnlyList<Guid>? ItemIds, IReadOnlyList<string>? GroupKeys, string Mode, bool AutoConfirm, IReadOnlyList<int>? ItemNs, bool Apply,
    bool Force, IReadOnlyList<string>? TriageRules = null, IReadOnlyList<Guid>? Reprocess = null)
{
    /// <summary>
    /// KB primeiro (o passo de IA cobre o que ficou sem correção), confirmação e aplicação automáticas, R1 gravada, cache de fontes aproveitado.
    /// <paramref name="triageRules"/>: regras de triagem do tipo de projeto da sessão (<c>null</c> = todas, runs antigos). <paramref name="reprocess"/>:
    /// itens confirmados com erro de gravação que entram para reaplicar (sem nova proposta).
    /// </summary>
    public static AutoRemediationParameters For(Guid sessionId, IReadOnlyList<Guid>? itemIds, IReadOnlyList<string>? triageRules = null,
        IReadOnlyList<Guid>? reprocess = null) =>
        new(sessionId, itemIds is { Count: > 0 } ? itemIds : null, null, Modules.Remediation.Apply.ProposeEngine.ModeKb, true, null, true, false,
            triageRules?.Select(r => r.Trim().ToUpperInvariant()).Where(r => r.Length > 0).Distinct().ToList(),
            reprocess is { Count: > 0 } ? reprocess : null);
}
