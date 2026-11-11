using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Remediation;

/// <summary>
/// Regra do cadastro de triagem. <see cref="Key"/> é a identidade fixa (regra padrão: R1..R9 ou o produto MIGNOW, que escolhe a lógica do código e
/// entra na obs "Triagem R1: …" que a conferência da R1, a isenção, o relatório e o CSV da skill leem); <see cref="Code"/> é o código exibido e
/// gravado nas marcações dos itens, e pode ser renomeado.
/// Padrão (<see cref="BuiltIn"/>): a linha guarda o que o consultor mudou – código, descrição, escopo, ativa, objetos (R7/R9), parâmetros da lógica
/// (<see cref="Parameters"/>: tipos da R1, notas da R5, operações da R8, objetos do produto…) e o filtro adicional (nome do objeto e texto do check).
/// Nova: casa por critérios – objetos referenciados, nome do objeto (curinga <c>*</c>) e texto do check/mensagem, juntos – e grava o efeito com a obs.
/// </summary>
public sealed class TriageRule : EntityBase
{
    public const string EffectSignal = "signal";

    /// <summary>Efeitos aceitos numa regra nova (os mesmos das regras padrão).</summary>
    public static readonly IReadOnlyList<string> Effects = [RemediationStatus.Cancelled, RemediationStatus.ConfirmedKb, RemediationStatus.Exemption, EffectSignal];

    private TriageRule() { }

    public string Key { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Effect { get; private set; } = string.Empty;

    public TriageScope Scope { get; private set; }

    public bool Active { get; private set; } = true;

    public bool BuiltIn { get; private set; }

    public IReadOnlyList<string> RefObjects { get; private set; } = [];

    /// <summary>Parâmetros da lógica de uma regra padrão (nome → valores); só os nomes que a regra usa.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Parameters { get; private set; } = new Dictionary<string, IReadOnlyList<string>>();

    public string? ObjectPattern { get; private set; }

    public string? MessagePattern { get; private set; }

    public string? Obs { get; private set; }

    /// <summary>Linha de uma regra padrão (criada na primeira edição dela).</summary>
    public static TriageRule ForBuiltIn(string key, string effect)
    {
        var k = Guard.Required(key, "TriageRule.CodeRequired", 40).ToUpperInvariant();
        return new() { Key = k, Code = k, Effect = effect, BuiltIn = true };
    }

    public static TriageRule Create(string code, string description, string effect, TriageScope scope, bool active, IEnumerable<string>? refObjects,
        string? objectPattern, string? messagePattern, string? obs)
    {
        var c = NormalizeCode(code);
        var r = new TriageRule { Key = c, Code = c };
        r.UpdateCustom(description, effect, scope, active, refObjects, objectPattern, messagePattern, obs);
        return r;
    }

    /// <summary>Novo código exibido (a <see cref="Key"/> não muda; itens já triados ficam com o código antigo nas marcações).</summary>
    public void Rename(string code) => Code = NormalizeCode(code);

    /// <summary>
    /// Regra padrão: descrição, escopo, ativa, objetos referenciados (quando a regra usa), parâmetros da lógica e filtro adicional
    /// (nome do objeto e texto do check). O efeito fica no código.
    /// </summary>
    public void UpdateBuiltIn(string description, TriageScope scope, bool active, IEnumerable<string>? refObjects,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? parameters, string? objectPattern, string? messagePattern)
    {
        Description = Guard.Required(description, "TriageRule.DescriptionRequired", 500);
        Scope = scope;
        Active = active;
        RefObjects = Normalize(refObjects);
        Parameters = (parameters ?? new Dictionary<string, IReadOnlyList<string>>())
            .ToDictionary(p => p.Key, p => (IReadOnlyList<string>)Normalize(p.Value), StringComparer.Ordinal);
        ObjectPattern = Guard.Optional(objectPattern, 120)?.ToUpperInvariant();
        MessagePattern = Guard.Optional(messagePattern, 200);
    }

    public void UpdateCustom(string description, string effect, TriageScope scope, bool active, IEnumerable<string>? refObjects,
        string? objectPattern, string? messagePattern, string? obs)
    {
        if (BuiltIn)
            throw new DomainException("TriageRule.BuiltInCriteria", "Regra padrão: o efeito fica no código.");
        if (!Effects.Contains(effect))
            throw new DomainException("TriageRule.InvalidEffect", $"Efeito inválido: {effect}.");
        Description = Guard.Required(description, "TriageRule.DescriptionRequired", 500);
        Effect = effect;
        Scope = scope;
        Active = active;
        RefObjects = Normalize(refObjects);
        ObjectPattern = Guard.Optional(objectPattern, 120)?.ToUpperInvariant();
        MessagePattern = Guard.Optional(messagePattern, 200);
        Obs = Guard.Optional(obs, 500);
        if (RefObjects.Count == 0 && ObjectPattern is null && MessagePattern is null)
            throw new DomainException("TriageRule.CriteriaRequired", "Informe ao menos um critério.");
    }

    private static string NormalizeCode(string code) => Guard.Required(code, "TriageRule.CodeRequired", 40).ToUpperInvariant();

    private static List<string> Normalize(IEnumerable<string>? values) =>
        [.. (values ?? []).Select(v => v.Trim().ToUpperInvariant()).Where(v => v.Length > 0).Distinct()];
}
