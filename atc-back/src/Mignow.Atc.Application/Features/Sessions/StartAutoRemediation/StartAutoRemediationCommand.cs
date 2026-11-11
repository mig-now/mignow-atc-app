using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Sessions.StartAutoRemediation;

/// <summary>"Play" da remediação: run <c>AutoRemediation</c> (202) com os itens escolhidos (<see cref="ItemIds"/> vazio = todos os pendentes).</summary>
/// <remarks>As regras de triagem não se escolhem: o play aplica as que valem para o tipo de projeto da sessão (<c>TriageEngine.RulesFor</c>).</remarks>
public sealed record StartAutoRemediationCommand(Guid SessionId, IReadOnlyList<Guid>? ItemIds)
    : ICommand<Result<RunResponse>>;
