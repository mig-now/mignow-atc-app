using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Resultado da triagem: as regras (com os itens de cada uma) e o tipo de projeto usado (R9 só no upgrade).</summary>
public sealed record TriageResult(IReadOnlyList<TriageRuleResult> Rules, ProjectType ProjectType)
{
    public TriageRuleResult? Rule(string code) => Rules.FirstOrDefault(r => r.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
}
