using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.Triage;

/// <summary>Aplica as regras escolhidas (<c>--aplicar R1,R2</c>); R3/R4/R6 só sinalizam. Depois da R1, rode o run <c>ConfirmR1</c>.</summary>
public sealed record TriageApplyCommand(Guid SessionId, IReadOnlyList<string> Rules) : ICommand<Result<TriageResponse>>;
