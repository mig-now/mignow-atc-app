using Mignow.Atc.Application.Ports.Ai;

namespace Mignow.Atc.Infra.External.Ai.Fake;

/// <summary>
/// Resposta determinística da IA de desenvolvimento (provedor Fake) para um <see cref="AiRequest.Purpose"/>. Registrada
/// por varredura: cada módulo cria a sua classe (ex.: <c>Fake/RemediationFakeResponder.cs</c>) sem editar nada comum.
/// </summary>
public interface IFakeAiResponder
{
    /// <summary>Propósitos atendidos (ex.: <c>remediation.propose</c>).</summary>
    IReadOnlyCollection<string> Purposes { get; }

    /// <summary>JSON da resposta (validado/desserializado pelo gateway como se viesse do modelo).</summary>
    string Respond(AiRequest request);
}
