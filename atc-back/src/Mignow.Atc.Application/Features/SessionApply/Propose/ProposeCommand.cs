using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.SessionApply.Propose;

/// <summary>Run <c>Propose</c>: itens e/ou grupos (sem nenhum = todos os pendentes), <c>Mode</c> = <c>kb</c>|<c>ia</c>; <c>AutoConfirm</c> exige sessão em modo sem confirmação. Devolve 202 com o run.</summary>
public sealed record ProposeCommand(Guid SessionId, IReadOnlyList<Guid>? ItemIds, IReadOnlyList<string>? GroupKeys, string Mode, bool AutoConfirm) : ICommand<Result<RunResponse>>;
