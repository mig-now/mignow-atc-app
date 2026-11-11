using FluentAssertions;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.Remediation;

/// <summary>
/// Cadastro das regras de triagem: escopo "Todos" vale para todo projeto; específico (upgrade ou conversão), só para o tipo dele. O consultor
/// edita as padrão (descrição, escopo, ativa, objetos da R7/R9) e cria regras novas por critério; o motor segue o cadastro.
/// </summary>
public sealed class TriageRulesCatalogTests
{
    private static readonly IReadOnlyList<ProductRule> Products = new DefaultProductRules().Get();

    private static RemediationItem Item(int n, string obj, string? reference, string? message = null, string type = "PROG", string? classification = null) =>
        RemediationItem.Create(1, n, obj, type, reference, reference is null ? null : "TABL", "Check S/4", message, null, null, null, classification, "1", null, "ATC", "[]");

    private static IReadOnlyList<TriageRuleDefinition> With(params TriageRule[] rows) => TriageRuleSource.Merge(TriageEngine.Catalog(Products), rows);

    private static TriageResult Classify(ProjectType type, IReadOnlyList<TriageRuleDefinition> rules, params RemediationItem[] items) =>
        TriageEngine.Classify(items, new SourceIndex([]), type, Products, rules);

    [Fact]
    public void Catalogo_tem_o_escopo_de_cada_regra()
    {
        var catalog = TriageEngine.Catalog(Products);
        catalog.Single(r => r.Code == "R9").Scope.Should().Be(TriageScope.Upgrade);
        catalog.Where(r => r.Code != "R9").Should().OnlyContain(r => r.Scope == TriageScope.All && r.BuiltIn && r.Active);
        catalog.Where(r => r.UsesRefObjects).Select(r => r.Code).Should().Equal("R7", "R9");
    }

    [Fact]
    public void Conversao_recebe_so_as_de_todos_e_upgrade_recebe_tambem_a_R9()
    {
        TriageEngine.RulesFor(ProjectType.Conversion, Products).Select(r => r.Code).Should().NotContain("R9").And.Contain("MIGNOW_FSCM_PRODUCT");
        TriageEngine.RulesFor(ProjectType.Upgrade, Products).Select(r => r.Code).Should().Equal(TriageEngine.Catalog(Products).Select(r => r.Code));
    }

    [Theory]
    [InlineData(TriageScope.All, ProjectType.Conversion, true)]
    [InlineData(TriageScope.All, ProjectType.Upgrade, true)]
    [InlineData(TriageScope.Upgrade, ProjectType.Conversion, false)]
    [InlineData(TriageScope.Upgrade, ProjectType.Upgrade, true)]
    [InlineData(TriageScope.Conversion, ProjectType.Conversion, true)]
    [InlineData(TriageScope.Conversion, ProjectType.Upgrade, false)]
    public void Escopo_define_os_projetos(TriageScope scope, ProjectType type, bool applies) =>
        new TriageRuleDefinition("RX", "x", "signal", scope).AppliesTo(type).Should().Be(applies);

    [Fact]
    public void Regra_padrao_editada_muda_escopo_objetos_e_pode_ser_desativada()
    {
        var r9 = TriageRule.ForBuiltIn("R9", RemediationStatus.Exemption);
        r9.UpdateBuiltIn("R9 para todos", TriageScope.All, true, ["mara"], null, null, null);
        var r7 = TriageRule.ForBuiltIn("R7", RemediationStatus.Exemption);
        r7.UpdateBuiltIn("BSEG", TriageScope.All, false, ["BSEG"], null, null, null);
        var rules = With(r9, r7);

        var result = Classify(ProjectType.Conversion, rules, Item(1, "ZPROG", "MARA"), Item(2, "ZPROG", "BSEG"), Item(3, "ZPROG", "KONV"));
        result.Rule("R9")!.Items.Select(h => h.N).Should().Equal(1); // vale em conversão e só para MARA agora
        result.Rule("R7").Should().BeNull(); // desativada: o item da BSEG segue para as regras seguintes
        TriageEngine.RulesFor(ProjectType.Conversion, rules).Select(r => r.Code).Should().NotContain("R7");
        rules.Single(r => r.Code == "R9").Description.Should().Be("R9 para todos");
    }

    [Fact]
    public void Regra_nova_casa_por_todos_os_criterios_e_grava_o_efeito_com_a_obs()
    {
        var own = TriageRule.Create("zx1", "Legado YMIG", RemediationStatus.Cancelled, TriageScope.Conversion, true, ["vbuk"], "YMIG*", "obsolete", "Fora do escopo");
        var rules = With(own);
        rules.Last().Should().Match<TriageRuleDefinition>(r => r.Code == "ZX1" && !r.BuiltIn);

        var result = Classify(ProjectType.Conversion, rules,
            Item(1, "YMIG_A", "VBUK", "Table VBUK is obsolete"), Item(2, "ZMIG_A", "VBUK", "Table VBUK is obsolete"), Item(3, "YMIG_B", "VBUK", "outra"));
        var rule = result.Rule("ZX1")!;
        rule.Effect.Should().Be(RemediationStatus.Cancelled);
        rule.Items.Should().ContainSingle().Which.Should().Match<TriageHit>(h => h.N == 1 && h.Obs == "Fora do escopo");
        TriageEngine.StatusOf(rule).Should().Be(RemediationStatus.Cancelled);

        Classify(ProjectType.Upgrade, rules, Item(1, "YMIG_A", "VBUK", "obsolete")).Rule("ZX1").Should().BeNull(); // escopo só conversão
    }

    [Fact]
    public void Regra_nova_sem_criterio_e_recusada()
    {
        var act = () => TriageRule.Create("ZX2", "x", RemediationStatus.Exemption, TriageScope.All, true, [], null, null, null);
        act.Should().Throw<Mignow.Atc.Domain.Common.DomainException>().Which.Code.Should().Be("TriageRule.CriteriaRequired");
    }

    [Fact]
    public void Cada_regra_mostra_a_condicao_do_codigo_e_a_ordem_de_avaliacao()
    {
        var own = TriageRule.Create("ZX3", "x", RemediationStatus.Exemption, TriageScope.All, true, ["vbuk"], "YMIG*", "obsolete", null);
        var rules = With(own);
        foreach (var r in rules)
            TriageRuleExplainer.Condition(r, Products).Should().NotBeNullOrWhiteSpace().And.NotBe(r.Description, r.Code);
        TriageRuleExplainer.Condition(rules.Single(r => r.Code == "R9"), Products).Should().Contain("KONV").And.Contain("MB0A");
        TriageRuleExplainer.Condition(rules.Single(r => r.Code == "MIGNOW_FSCM_PRODUCT"), Products).Should().Contain("ZGFSCM_MIG_CREDITO");
        TriageRuleExplainer.Condition(rules.Single(r => r.Code == "ZX3"), Products).Should()
            .Be("O objeto referenciado está em: VBUK e o nome do objeto casa com YMIG* (curinga *) e o check ou a mensagem contém \"obsolete\" (sem diferenciar maiúsculas).");

        // mesma ordem do motor: produto, regra nova, R9, R7, R8, R6, R1, R5, R4, R2, R3
        TriageRuleExplainer.Order(rules, Products).OrderBy(kv => kv.Value).Select(kv => kv.Key).Should()
            .Equal("MIGNOW_FSCM_PRODUCT", "ZX3", "R9", "R7", "R8", "R6", "R1", "R5", "R4", "R2", "R3");
    }

    [Fact]
    public void Regra_padrao_renomeada_marca_com_o_codigo_novo_e_mantem_a_obs_da_chave()
    {
        var r1 = TriageRule.ForBuiltIn("R1", RemediationStatus.Cancelled);
        r1.UpdateBuiltIn("sem código", TriageScope.All, true, null, null, null, null);
        r1.Rename("zr1");
        var rules = With(r1);
        var result = Classify(ProjectType.Conversion, rules, Item(1, "ZTAB", null, type: "TABL", classification: "Ambiguous"));
        var rule = result.Rule("ZR1")!;
        rule.Key.Should().Be("R1");
        rule.Items.Single().Obs.Should().StartWith("Triagem R1:"); // a conferência da R1, a isenção e o relatório leem a obs pela chave
        result.Rule("R1").Should().BeNull();
        TriageRuleExplainer.Order(rules, Products)["R1"].Should().Be(6);
    }

    [Fact]
    public void Parametros_da_logica_mudam_o_que_a_regra_padrao_verifica()
    {
        var r8 = TriageRule.ForBuiltIn("R8", RemediationStatus.Exemption);
        r8.UpdateBuiltIn("só UPDATE", TriageScope.All, true, null,
            new Dictionary<string, IReadOnlyList<string>> { ["operations"] = ["update"], ["refTypes"] = ["TABL"] }, null, null);
        var r1 = TriageRule.ForBuiltIn("R1", RemediationStatus.Cancelled);
        r1.UpdateBuiltIn("só TABL", TriageScope.All, true, null,
            new Dictionary<string, IReadOnlyList<string>> { ["objectTypes"] = ["TABL"], ["classifications"] = ["ambiguous", "probable"] }, null, null);
        var rules = With(r8, r1);

        var result = Classify(ProjectType.Conversion, rules,
            Item(1, "ZPROG", "MARA", "DB OPERATION UPDATE on MARA"), Item(2, "ZPROG", "MARC", "DB OPERATION MODIFY on MARC"),
            Item(3, "ZTAB", null, type: "TABL", classification: "Probable"), Item(4, "ZVIEW", null, type: "VIEW", classification: "Ambiguous"));
        result.Rule("R8")!.Items.Select(h => h.N).Should().Equal(1);
        result.Rule("R1")!.Items.Select(h => h.N).Should().Equal(3);
        TriageRuleExplainer.Condition(rules.Single(r => r.RuleKey == "R8"), Products).Should().Contain("seguido de UPDATE").And.Contain("tipo TABL");
    }

    [Fact]
    public void Filtro_adicional_restringe_a_regra_padrao()
    {
        var r6 = TriageRule.ForBuiltIn("R6", TriageRuleResult.Signal);
        r6.UpdateBuiltIn("standard SAPM", TriageScope.All, true, null, null, "SAPM*", null);
        var rules = With(r6);
        var result = Classify(ProjectType.Conversion, rules, Item(1, "SAPMV45A", "VBAK"), Item(2, "RM07MLBS", "VBAK"));
        result.Rule("R6")!.Items.Select(h => h.N).Should().Equal(1);
        TriageRuleExplainer.Condition(rules.Single(r => r.RuleKey == "R6"), Products).Should().EndWith("Filtro adicional: o nome do objeto casa com SAPM* (curinga *).");
    }
}
