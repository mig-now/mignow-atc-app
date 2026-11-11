namespace Mignow.Atc.Infra.External.Sap.Simulator;

public sealed partial class SimulatedSapState
{
    private readonly Dictionary<string, SimulatedExemption> _exemptions = new(StringComparer.Ordinal);

    /// <summary>Política do módulo Exemption (padrões quando o cenário não traz a chave).</summary>
    public ExemptionScenario ExemptionConfig => Scenario.Exemption ?? new ExemptionScenario();

    /// <summary>Pedidos aceitos (markerId → pedido). Chamar dentro de <c>WithLock</c>.</summary>
    public IReadOnlyDictionary<string, SimulatedExemption> Exemptions => _exemptions;

    public void AddExemption(string markerId, SimulatedExemption exemption) => _exemptions[markerId] = exemption;

    public bool HasExemption(string markerId) => _exemptions.ContainsKey(markerId);
}

/// <summary>Pedido de isenção aceito pelo simulador: quem pediu, aprovador, motivo, justificativa e o XML recebido.</summary>
public sealed record SimulatedExemption(string RequestedBy, string Approver, string Reason, string Justification, string RequestXml);
