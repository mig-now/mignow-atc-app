namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Módulo Extraction: ajustes da execução do ATC simulada (chave <c>atcExtraction</c> do cenário, opcional). A execução sai de
/// <c>atcRuns[0]</c> + <c>atcRules</c> aplicadas ao fonte dos objetos de cliente (um finding por linha que casa).
/// <see cref="BaselineEvery"/>: a cada N findings um tem entrada de baseline (0 = nenhum); <see cref="InProgressRun"/>
/// acrescenta uma execução mais nova ainda em andamento. <see cref="MissingTables"/>: tabelas opcionais ausentes no sistema
/// (<c>SATC_AC_BSI</c>, <c>CWBNTSTXT</c>), como no MI9 sem baseline do ATC.
/// </summary>
public sealed record ScenarioExtraction(string Series = "ZMIG_SIM", int BaselineEvery = 5, bool InProgressRun = false, string[]? MissingTables = null);

public sealed partial record SapScenario
{
    public ScenarioExtraction AtcExtraction { get; init; } = new();
}
