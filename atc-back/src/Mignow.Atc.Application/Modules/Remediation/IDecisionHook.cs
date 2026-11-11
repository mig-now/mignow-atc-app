using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Ponto de extensão da decisão (PARTE 2): chamado para cada item depois de <c>MarkStatus</c> e antes do commit. A parte 2 implementa (e registra em
/// <c>RemediationApplyModule</c>) o que vai além de status/obs/ficha: <c>ConfirmIa</c> grava o cartão/fatos na KB e vincula à proposta
/// (<see cref="DecisionContext.Replaces"/>/<see cref="DecisionContext.Reason"/> = substitui/motivo). Falha cancela a decisão inteira (sem commit).
/// </summary>
public interface IDecisionHook
{
    Task<Result> OnDecidedAsync(DecisionContext context, CancellationToken ct);
}
