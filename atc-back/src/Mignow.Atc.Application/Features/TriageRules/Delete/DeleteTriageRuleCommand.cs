using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.TriageRules.Delete;

/// <summary>Exclui uma regra nova. Regra padrão não se exclui (desativar pelo cadastro).</summary>
public sealed record DeleteTriageRuleCommand(string Code) : ICommand<Result>;
