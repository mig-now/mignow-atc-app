using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Scan;

/// <summary>
/// Origem do <c>regras.json</c> da varredura (somente leitura). O caminho vem de <c>GeneralSettings.MignowRulesPath</c>
/// (absoluto ou relativo à raiz do repositório).
/// </summary>
public interface IMignowRulesSource
{
    /// <summary>Texto JSON do arquivo de regras; falha com <c>Scan.RulesNotFound</c> se não existir.</summary>
    Task<Result<string>> ReadAsync(string path, CancellationToken ct);
}
