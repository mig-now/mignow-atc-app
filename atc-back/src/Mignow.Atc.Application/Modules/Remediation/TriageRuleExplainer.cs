namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// O que cada regra verifica de fato no <see cref="TriageEngine"/> (texto da condição do código) e a ordem de avaliação: só itens pendentes,
/// e o item fica com a primeira regra que casar – produtos MIGNOW, regras novas, R9, R7, R8, R6, R1 e, com o fonte lido, R5, R4, R2, R3.
/// </summary>
public static class TriageRuleExplainer
{
    private static readonly string[] BuiltInOrder = ["R9", "R7", "R8", "R6", "R1", "R5", "R4", "R2", "R3"];

    /// <summary>Posição (1 = primeira) de cada regra na avaliação, pela chave fixa (<see cref="TriageRuleDefinition.RuleKey"/>).</summary>
    public static IReadOnlyDictionary<string, int> Order(IReadOnlyList<TriageRuleDefinition> rules, IReadOnlyList<ProductRule> products)
    {
        var custom = rules.Where(r => !r.BuiltIn).Select(r => r.RuleKey);
        return products.Select(p => p.Name).Concat(custom).Concat(BuiltInOrder).Select((key, i) => (key, i))
            .ToDictionary(x => x.key, x => x.i + 1, StringComparer.OrdinalIgnoreCase);
    }

    public static string Condition(TriageRuleDefinition rule, IReadOnlyList<ProductRule> products)
    {
        if (!rule.BuiltIn)
            return Custom(rule);
        var main = Main(rule, products);
        var extra = Criteria(rule, includeRefs: false);
        return extra.Length == 0 ? main : $"{main} Filtro adicional: {extra}.";
    }

    private static string List(TriageRuleDefinition rule, string name) => string.Join(", ", rule.Param(name));

    private static string Main(TriageRuleDefinition rule, IReadOnlyList<ProductRule> products)
    {
        var refs = string.Join(", ", rule.Refs);
        if (products.Any(p => p.Name.Equals(rule.RuleKey, StringComparison.OrdinalIgnoreCase)))
            return $"O objeto do finding é do produto MIGNOW: {List(rule, "objects")}.";
        return rule.RuleKey.ToUpperInvariant() switch
        {
            "R9" or "R7" => $"O objeto referenciado está em: {refs}.",
            "R8" => $"A mensagem do finding contém \"DB OPERATION\" seguido de {List(rule, "operations")} e o objeto referenciado é do tipo "
                + $"{List(rule, "refTypes")} e standard (nome fora do padrão do cliente: Z, Y ou /namespace/).",
            "R6" => "O objeto do finding é standard: o nome não começa por Z, Y ou /namespace/ (aceita os prefixos SAPM, SAPL, L e M antes deles).",
            "R1" => $"O tipo do objeto está em: {List(rule, "objectTypes")} (sem código ABAP) e a classificação do finding é {List(rule, "classifications")}.",
            "R5" => $"O objeto referenciado é função (FUNC) chamada no fonte lido; o finding é da nota {List(rule, "notes")} ou o texto contém "
                + $"\"{string.Join("\" ou \"", rule.Param("texts"))}\"; e em todas as CALL FUNCTION dela os parâmetros de material (material/matnr) "
                + "passados são só os *_LONG.",
            "R4" => "O objeto referenciado aparece em linha ativa do fonte lido e também dentro de bloco DELETION do marcador \"S/4 MIGRATION\".",
            "R2" => "O objeto referenciado não aparece em linha ativa do fonte lido, só dentro de bloco DELETION do marcador \"S/4 MIGRATION\".",
            "R3" => "O objeto referenciado não aparece no fonte lido (nem em linha ativa nem em bloco DELETION).",
            _ => rule.Description,
        };
    }

    private static string Custom(TriageRuleDefinition rule)
    {
        var joined = Criteria(rule, includeRefs: true);
        return joined.Length == 0 ? string.Empty : char.ToUpperInvariant(joined[0]) + joined[1..] + ".";
    }

    private static string Criteria(TriageRuleDefinition rule, bool includeRefs)
    {
        var parts = new List<string>();
        if (includeRefs && rule.Refs.Count > 0)
            parts.Add($"o objeto referenciado está em: {string.Join(", ", rule.Refs)}");
        if (rule.ObjectPattern is { Length: > 0 } pattern)
            parts.Add($"o nome do objeto casa com {pattern} (curinga *)");
        if (rule.MessagePattern is { Length: > 0 } text)
            parts.Add($"o check ou a mensagem contém \"{text}\" (sem diferenciar maiúsculas)");
        return string.Join(" e ", parts);
    }
}
