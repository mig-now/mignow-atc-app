namespace Mignow.Atc.Application.Ports.Secrets;

/// <summary>
/// Cofre de segredos (senhas SAP, chaves de API). O cadastro guarda só a referência. Local: cifrado no banco
/// (AES-256-GCM, chave fora do banco); produção: Key Vault (F7). O valor nunca volta pela API nem vai para log.
/// </summary>
public interface ISecretStore
{
    /// <summary>Grava o valor e devolve a referência (nova, ou a informada).</summary>
    Task<string> SetAsync(string? secretRef, string purpose, string value, CancellationToken ct);

    Task<string?> GetAsync(string secretRef, CancellationToken ct);

    /// <summary>Apaga o segredo (o cadastro que o usava foi apagado).</summary>
    Task DeleteAsync(string secretRef, CancellationToken ct);
}
