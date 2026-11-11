using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Reuse;

/// <summary>
/// Reaproveita fichas confirmadas de outra sessão do mesmo ambiente (<c>reaproveitar</c>). Só avalia e devolve os candidatos (trecho atual, novo e
/// bloco com marcadores); não altera status nem grava nada. <c>ItemNs</c> vazio = todos os pendentes.
/// </summary>
public sealed record ReuseSessionQuery(Guid SessionId, Guid FromSessionId, IReadOnlyList<int>? ItemNs) : IQuery<Result<ReuseResult>>;
