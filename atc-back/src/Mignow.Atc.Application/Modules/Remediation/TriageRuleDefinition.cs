using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Regra do cadastro de triagem, já com o que o consultor gravou. <c>Effect</c> = status gravado ao aplicar, ou <c>signal</c> (só sinaliza).
/// <see cref="RuleKey"/> = identidade fixa (lógica do código e obs "Triagem R1: …"); <see cref="Code"/> = código exibido/gravado nas marcações.
/// <see cref="UsesRefObjects"/>: regra padrão cuja lista de objetos referenciados se edita (R7, R9). <see cref="Parameters"/>: parâmetros da lógica
/// de uma regra padrão (os nomes que ela aceita são os do padrão). <see cref="ObjectPattern"/>/<see cref="MessagePattern"/>: critério da regra nova
/// ou filtro adicional da regra padrão (os informados valem juntos).
/// </summary>
public sealed record TriageRuleDefinition(string Code, string Description, string Effect, TriageScope Scope, bool BuiltIn = true, bool Active = true,
    IReadOnlyList<string>? RefObjects = null, string? ObjectPattern = null, string? MessagePattern = null, string? Obs = null, bool UsesRefObjects = false,
    string? Key = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? Parameters = null)
{
    public IReadOnlyList<string> Refs => RefObjects ?? [];

    public string RuleKey => Key ?? Code;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Params => Parameters ?? new Dictionary<string, IReadOnlyList<string>>();

    public IReadOnlyList<string> Param(string name) => Params.TryGetValue(name, out var v) ? v : [];

    /// <summary>"Todos" vale para todo projeto; específica, só para o tipo de projeto dela.</summary>
    public bool AppliesTo(ProjectType projectType) => Scope switch
    {
        TriageScope.Upgrade => projectType == ProjectType.Upgrade,
        TriageScope.Conversion => projectType == ProjectType.Conversion,
        _ => true,
    };
}
