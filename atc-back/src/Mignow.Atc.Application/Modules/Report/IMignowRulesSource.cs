namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Lê as regras da varredura MIGNOW (arquivo apontado por <c>GeneralSettings.MignowRulesPath</c>); vazio se o arquivo não existir.</summary>
public interface IMignowRulesSource
{
    Task<IReadOnlyDictionary<string, MignowRuleInfo>> LoadAsync(string path, CancellationToken ct);
}
