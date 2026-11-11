using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Sessions.Triage;

/// <summary>Regra da triagem com os itens afetados. <c>Effect</c> = status gravado ao aplicar, ou <c>signal</c> (só sinaliza).
/// <c>Key</c> = identidade fixa da regra (R1..R9, produto), mesmo com o código renomeado no cadastro.</summary>
public sealed record TriageRuleResponse(string Code, string Description, string Effect, int Count, IReadOnlyList<TriageHit> Items, string? Key = null);

/// <summary>Prévia (<c>Applied</c> nulo) ou resultado da aplicação (<c>Applied</c> = nºs dos itens marcados).</summary>
public sealed record TriageResponse(ProjectType ProjectType, IReadOnlyList<TriageRuleResponse> Rules, int Pending, IReadOnlyList<int>? Applied);
