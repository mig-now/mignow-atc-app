using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.TriageRules.List;

/// <summary>Cadastro das regras de triagem. Com <see cref="ProjectType"/>, só as ativas que valem para esse tipo de projeto (as que o play aplica).</summary>
public sealed record ListTriageRulesQuery(ProjectType? ProjectType = null) : IQuery<Result<IReadOnlyList<TriageRuleDefinitionResponse>>>;
