namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Seção <c>Sap:Simulator</c>. O simulador só responde em Development, Local e testes.</summary>
public sealed class SapSimulatorOptions
{
    public const string Section = "Sap:Simulator";

    /// <summary>Pasta dos cenários JSON (relativa ao diretório da aplicação ou absoluta).</summary>
    public string ScenariosPath { get; set; } = "sap-scenarios";
}
