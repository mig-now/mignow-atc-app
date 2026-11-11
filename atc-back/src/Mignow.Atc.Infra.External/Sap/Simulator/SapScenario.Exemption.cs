namespace Mignow.Atc.Infra.External.Sap.Simulator;

public sealed partial record SapScenario
{
    /// <summary>Módulo Exemption: política de isenção do sistema simulado (opcional; sem a chave valem os padrões).</summary>
    public ExemptionScenario? Exemption { get; init; }
}
