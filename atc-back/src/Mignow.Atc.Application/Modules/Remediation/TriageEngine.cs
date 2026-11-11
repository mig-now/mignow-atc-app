using System.Text.RegularExpressions;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Porte de <c>cmd_triagem</c> do atc.py (sem gravar): classifica os itens pendentes por regra, na ordem do script (produto MIGNOW, R9, R7, R8,
/// R6, R1, depois a análise do fonte lido: R5, R4, R2, R3). Cada item cai em no máximo uma regra.
/// R1 sem código ABAP + Ambiguous → Cancelado · R2 só em bloco DELETION → Confirmado KB · R3 uso não localizado (sinaliza) · R4 uso ativo + DELETION
/// (sinaliza) · R5 nota 2438131 com *_LONG → Isenção · R6 objeto standard (sinaliza) · R7 BSEG → Isenção (funcional) · R8 escrita em tabela
/// standard → Isenção · R9 upgrade: objeto referenciado fora do escopo → Isenção (antes da R7) · produto MIGNOW → Cancelado.
/// O cadastro (<see cref="TriageRuleSource"/>) decide quais valem (ativa + escopo do tipo de projeto), os objetos da R7/R9 e as regras novas,
/// avaliadas logo depois dos produtos MIGNOW, na ordem de criação.
/// </summary>
public static partial class TriageEngine
{
    public const string ObsR8 = "Out Of Scope - Analise do Cliente";

    /// <summary>R9: objetos referenciados isentos no projeto de upgrade (decisões do consultor 29 e 31.10.2026).</summary>
    public static readonly IReadOnlyList<string> UpgradeExemptRefs = ["BSEG", "KNKK", "S066", "S067", "KONV", "CKMLCR", "CKMLPP", "MB0A"];

    private static readonly TriageRuleDefinition[] FixedRules =
    [
        new("R1", "sem código + Ambiguous → Cancelado", RemediationStatus.Cancelled, TriageScope.All,
            Parameters: Params(("objectTypes", [.. StandardObjects.NoCodeTypes.Order(StringComparer.Ordinal)]), ("classifications", ["AMBIGUOUS"]))),
        new("R2", "já corrigido → Confirmado KB", RemediationStatus.ConfirmedKb, TriageScope.All),
        new("R3", "uso não localizado → só sinaliza", TriageRuleResult.Signal, TriageScope.All),
        new("R4", "uso ativo + bloco DELETION do mesmo objeto → só sinaliza (conferir se já corrigido)", TriageRuleResult.Signal, TriageScope.All),
        new("R5", "nota 2438131 com *_LONG em todas as chamadas → Isenção (pedido via atc-isencao)", RemediationStatus.Exemption, TriageScope.All,
            Parameters: Params(("notes", ["2438131"]), ("texts", ["MATERIAL NUMBER FIELD LENGTH EXTENSION"]))),
        new("R6", "objeto standard (modificação/exit) → só sinaliza; fim da fila, leitura com confirmação", TriageRuleResult.Signal, TriageScope.All),
        new("R7", "objeto referenciado BSEG → Isenção, obs \"pendente para o funcional\"", RemediationStatus.Exemption, TriageScope.All,
            RefObjects: ["BSEG"], UsesRefObjects: true),
        new("R8", $"MODIFY/UPDATE/INSERT em tabela standard → Isenção, obs \"{ObsR8}\"", RemediationStatus.Exemption, TriageScope.All,
            Parameters: Params(("operations", ["MODIFY", "UPDATE", "INSERT"]), ("refTypes", ["TABL", "VIEW"]))),
        new("R9", $"upgrade: objeto referenciado {string.Join('/', UpgradeExemptRefs)} → Isenção", RemediationStatus.Exemption, TriageScope.Upgrade,
            RefObjects: UpgradeExemptRefs, UsesRefObjects: true),
    ];

    /// <summary>Status que a regra grava ao ser aplicada (<c>null</c> = a regra só sinaliza).</summary>
    public static string? StatusOf(TriageRuleResult rule) => rule.Effect == TriageRuleResult.Signal ? null : rule.Effect;

    /// <summary>Regras padrão (R1..R9 + produtos MIGNOW, todos os projetos), na ordem do script, sem o que o consultor gravou no cadastro.</summary>
    public static IReadOnlyList<TriageRuleDefinition> Catalog(IReadOnlyList<ProductRule> products) =>
    [
        .. FixedRules,
        .. products.Select(p => new TriageRuleDefinition(p.Name, $"produto MIGNOW ({p.Description}) → Cancelado", RemediationStatus.Cancelled, TriageScope.All,
            Parameters: Params(("objects", [.. p.Objects.Order(StringComparer.Ordinal)])))),
    ];

    private static Dictionary<string, IReadOnlyList<string>> Params(params (string Name, IReadOnlyList<string> Values)[] values) =>
        values.ToDictionary(v => v.Name, v => v.Values, StringComparer.Ordinal);

    /// <summary>Regras ativas que valem para o tipo de projeto (escopo "Todos" + as específicas dele).</summary>
    public static IReadOnlyList<TriageRuleDefinition> RulesFor(ProjectType projectType, IReadOnlyList<TriageRuleDefinition> rules) =>
        [.. rules.Where(r => r.Active && r.AppliesTo(projectType))];

    /// <summary>Regras padrão que valem para o tipo de projeto (sem o cadastro gravado).</summary>
    public static IReadOnlyList<TriageRuleDefinition> RulesFor(ProjectType projectType, IReadOnlyList<ProductRule> products) =>
        RulesFor(projectType, Catalog(products));

    public static TriageResult Classify(IReadOnlyList<RemediationItem> items, SourceIndex sources, ProjectType projectType, IReadOnlyList<ProductRule> products) =>
        Classify(items, sources, projectType, products, Catalog(products));

    /// <summary>Classifica com o cadastro <paramref name="rules"/> (padrão + gravado): só as regras ativas do tipo de projeto valem.</summary>
    public static TriageResult Classify(IReadOnlyList<RemediationItem> items, SourceIndex sources, ProjectType projectType, IReadOnlyList<ProductRule> products,
        IReadOnlyList<TriageRuleDefinition> rules)
    {
        var applicable = RulesFor(projectType, rules);
        // lógica pela chave fixa (R1..R9, produto); itens marcados com o código exibido
        var on = applicable.ToDictionary(r => r.RuleKey, StringComparer.OrdinalIgnoreCase);
        var custom = applicable.Where(r => !r.BuiltIn).ToList();
        var buckets = applicable.ToDictionary(r => r.RuleKey, _ => new List<TriageHit>(), StringComparer.OrdinalIgnoreCase);
        var r8 = on.GetValueOrDefault("R8");
        var writeRegex = r8 is null || r8.Param("operations").Count == 0 ? null
            : new Regex(@"DB\s+OPERATION\s+(" + string.Join('|', r8.Param("operations").Select(Regex.Escape)) + @")\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        foreach (var it in items)
        {
            if (it.Status.Length > 0)
                continue;
            var obj = it.ObjectName;
            var type = it.ObjectType;
            var reference = it.RefObject ?? string.Empty;
            var robt = (it.RefType ?? string.Empty).ToUpperInvariant();
            var message = it.Message ?? string.Empty;
            TriageHit Hit(string obs, string? operation = null, IReadOnlyList<string>? calls = null) =>
                new(it.N, obj, type, reference, obs, operation, calls);
            // regra padrão ativa do projeto e, se tiver, o filtro adicional (nome do objeto / texto do check) casando com o item
            bool On(string key) => on.TryGetValue(key, out var d) && Filter(d, it);

            var product = products.FirstOrDefault(p => On(p.Name) && on[p.Name].Param("objects").Contains(obj.ToUpperInvariant()));
            if (product is not null)
            {
                buckets[product.Name].Add(Hit($"Triagem {product.Name}: objeto do produto MIGNOW ({product.Description}) – finding ignorado"));
                continue;
            }
            if (custom.FirstOrDefault(r => Matches(r, it)) is { } own)
            {
                buckets[own.RuleKey].Add(Hit(own.Obs ?? $"Triagem {own.Code}: {own.Description}"));
                continue;
            }
            if (On("R9") && on["R9"].Refs.Contains(reference.ToUpperInvariant()))
            {
                buckets["R9"].Add(Hit($"Triagem R9: projeto de upgrade – {reference.ToUpperInvariant()} isento (sem remediação no upgrade; decisão do consultor)"));
                continue;
            }
            if (On("R7") && on["R7"].Refs.Contains(reference.ToUpperInvariant()))
            {
                buckets["R7"].Add(Hit($"Triagem R7: {reference.ToUpperInvariant()} – pendente para o funcional (avaliação de negócio; sem correção técnica nesta remediação)"));
                continue;
            }
            var write = writeRegex?.Match(message);
            if (On("R8") && write is { Success: true } && reference.Length > 0 && r8!.Param("refTypes").Contains(robt) && StandardObjects.IsStandard(reference))
            {
                buckets["R8"].Add(Hit(ObsR8, write.Groups[1].Value.ToUpperInvariant()));
                continue;
            }
            if (On("R6") && StandardObjects.IsStandard(obj))
            {
                buckets["R6"].Add(Hit("Triagem R6: objeto standard (modificação/exit) – deixado para o final"));
                continue;
            }
            if (On("R1") && on["R1"].Param("objectTypes").Contains(type.ToUpperInvariant())
                && on["R1"].Param("classifications").Contains((it.Classification ?? string.Empty).ToUpperInvariant()))
            {
                buckets["R1"].Add(Hit("Triagem R1: objeto sem código ABAP, finding Ambiguous (confirmar na SE11)"));
                continue;
            }
            if (reference.Length == 0)
                continue;
            var hits = sources.Locate(obj, reference);
            if (hits is null)
                continue;
            var deletion = sources.UsageInDeletion(obj, reference);
            if (On("R5") && hits.Count > 0 && robt == "FUNC" && (on["R5"].Param("notes").Contains((it.Note ?? string.Empty).ToUpperInvariant())
                || on["R5"].Param("texts").Any(t => (it.Text ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase))))
            {
                var (ok, calls) = sources.MatnrLongOk(obj, reference);
                if (ok)
                {
                    buckets["R5"].Add(Hit("Triagem R5: já corrigido (MATERIAL_LONG em todas as chamadas); o check marca qualquer chamada da BAPI – solicitar isenção ATC (falso positivo)",
                        calls: calls));
                    continue;
                }
            }
            if (hits.Count > 0)
            {
                if (On("R4") && deletion.Count > 0)
                {
                    var active = string.Join(", ", hits.Take(3).Select(h => $"{h.Source}:{h.Line}"));
                    buckets["R4"].Add(Hit($"Triagem R4: uso ativo em {active}; já existe bloco DELETION com o objeto ({string.Join(", ", deletion.Take(3))}) – conferir se já foi corrigido"));
                }
                continue;
            }
            if (deletion.Count > 0)
            {
                if (On("R2"))
                    buckets["R2"].Add(Hit($"Triagem R2: já corrigido no sistema (uso só em bloco DELETION: {string.Join(", ", deletion.Take(3))})"));
            }
            else if (On("R3"))
                buckets["R3"].Add(Hit("Triagem R3: uso do objeto referenciado não localizado no fonte"));
        }
        var result = applicable.Select(r => new TriageRuleResult(r.Code, r.Description, r.Effect, buckets[r.RuleKey], r.RuleKey)).ToList();
        return new TriageResult(result, projectType);
    }

    /// <summary>Regra nova: todos os critérios informados casam (objetos referenciados, nome do objeto com curinga *, texto do check/mensagem).</summary>
    public static bool Matches(TriageRuleDefinition rule, RemediationItem item)
    {
        if (rule.Refs.Count > 0 && !rule.Refs.Contains((item.RefObject ?? string.Empty).ToUpperInvariant()))
            return false;
        if (rule.ObjectPattern is { Length: > 0 } pattern && !ObjectMatches(pattern, item.ObjectName))
            return false;
        if (rule.MessagePattern is { Length: > 0 } text && !TextMatches(text, item))
            return false;
        return rule.Refs.Count > 0 || rule.ObjectPattern is { Length: > 0 } || rule.MessagePattern is { Length: > 0 };
    }

    /// <summary>Filtro adicional de uma regra padrão: nome do objeto (curinga *) e texto do check/mensagem, quando informados.</summary>
    public static bool Filter(TriageRuleDefinition rule, RemediationItem item) =>
        !rule.BuiltIn || ((rule.ObjectPattern is not { Length: > 0 } p || ObjectMatches(p, item.ObjectName))
            && (rule.MessagePattern is not { Length: > 0 } t || TextMatches(t, item)));

    private static bool ObjectMatches(string pattern, string objectName) =>
        Regex.IsMatch(objectName, "^" + Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal) + "$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static bool TextMatches(string text, RemediationItem item) =>
        (item.Message ?? string.Empty).Contains(text, StringComparison.OrdinalIgnoreCase) || (item.Check ?? string.Empty).Contains(text, StringComparison.OrdinalIgnoreCase);
}
