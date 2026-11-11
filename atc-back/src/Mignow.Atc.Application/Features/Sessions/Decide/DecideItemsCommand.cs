using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Decide;

/// <summary>Decisão do consultor (síncrona) sobre itens. <c>Replaces</c>/<c>Reason</c> (substitui/motivo, só ConfirmIa) seguem para os <see cref="IDecisionHook"/>.</summary>
public sealed record DecideItemsCommand(Guid SessionId, IReadOnlyList<Guid> ItemIds, SessionDecision Decision, string? Obs, IReadOnlyList<string>? Replaces, string? Reason)
    : ICommand<Result<DecideItemsResponse>>;

/// <summary>Itens gravados e a situação da sessão depois da decisão. <c>Skipped</c>: itens sem sugestão a aceitar (ficaram como estavam).</summary>
public sealed record DecideItemsResponse(IReadOnlyList<ItemResponse> Items, int Pending, int Skipped = 0);
