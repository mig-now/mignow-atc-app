namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>Propósitos das chamadas de IA do módulo (<see cref="Mignow.Atc.Application.Ports.Ai.AiRequest.Purpose"/>): vão para o log e escolhem a resposta Fake.</summary>
public static class RemediationPurposes
{
    public const string Propose = "remediation.propose";
    public const string Review = "remediation.review";
}
