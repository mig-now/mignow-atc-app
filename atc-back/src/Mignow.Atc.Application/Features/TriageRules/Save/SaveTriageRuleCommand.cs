using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.TriageRules.Save;

/// <summary>
/// Cria (<see cref="CurrentCode"/> nulo) ou altera a regra de código <see cref="CurrentCode"/>. <see cref="Code"/> = código novo (na alteração,
/// renomeia; vazio mantém). Regra padrão: código, descrição, escopo, ativa, objetos (R7/R9), <see cref="Parameters"/> da lógica e filtro adicional
/// (<see cref="ObjectPattern"/>, <see cref="MessagePattern"/>); o efeito e a obs ficam no código. Regra nova: efeito + ao menos um critério.
/// </summary>
public sealed record SaveTriageRuleCommand(string? CurrentCode, string? Code, string Description, string Effect, TriageScope Scope, bool Active,
    IReadOnlyList<string>? RefObjects, string? ObjectPattern, string? MessagePattern, string? Obs,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? Parameters = null) : ICommand<Result<TriageRuleDefinitionResponse>>
{
    public bool IsNew => CurrentCode is null;
}
