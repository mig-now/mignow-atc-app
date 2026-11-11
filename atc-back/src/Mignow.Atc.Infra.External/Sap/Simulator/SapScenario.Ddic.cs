namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>
/// Módulo Ddic do cenário (chave <c>ddicObjects</c> no JSON, opcional): objetos do dicionário com o DDL/definição.
/// <c>Kind</c> = STRU | TABL | FUNC | DTEL (só STRU/TABL são graváveis).
/// </summary>
public sealed partial record SapScenario
{
    public IReadOnlyList<ScenarioDdicObject> DdicObjects { get; init; } = [];
}
