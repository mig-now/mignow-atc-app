namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Cenário do simulador (JSON em <c>sap-scenarios/</c>): objetos, requests, regras de sintaxe/ATC, execuções do ATC,
/// categoria do mandante (T000) e tabelas para o SQL simulado. <c>partial</c>: módulos acrescentam propriedades em
/// <c>SapScenario.&lt;Módulo&gt;.cs</c> (chaves novas no JSON, opcionais).
/// </summary>
public sealed partial record SapScenario
{
    public string Release { get; init; } = "S/4HANA (simulado)";

    /// <summary>T000-CCCATEGORY do mandante simulado; vazio = ilegível (a escrita é bloqueada).</summary>
    public string? ClientCategory { get; init; } = "T";

    public IReadOnlyList<ScenarioObject> Objects { get; init; } = [];

    public IReadOnlyList<ScenarioRequest> Requests { get; init; } = [];

    public IReadOnlyList<ScenarioRule> SyntaxRules { get; init; } = [];

    public IReadOnlyList<ScenarioRule> AtcRules { get; init; } = [];

    public IReadOnlyList<ScenarioAtcRun> AtcRuns { get; init; } = [];

    /// <summary>Tabelas para o SQL simulado (<c>SELECT … FROM tabela WHERE campo = 'valor' AND …</c>): nome → linhas.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>> Tables { get; init; } =
        new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>();
}
