using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap.Ddic;

/// <summary>
/// Estruturas e tabelas Z em fonte DDL (porte de <c>aplicar_ddic.ps1</c> + <c>Adt.psm1</c>). A escrita passa pela mesma trava do
/// <see cref="ISapWriter"/> (produção/T000=P/ilegível bloqueiam). As regras (estrutura da tabela, backup, diff) ficam no serviço.
/// </summary>
public interface IDdicWriter
{
    /// <summary>Lê o DDL ativo (somente leitura); objeto inexistente = <c>Exists</c> falso.</summary>
    Task<Result<DdicObjectState>> ReadObjectAsync(SapConnection connection, DdicKind kind, string name, CancellationToken ct);

    /// <summary>Valida nome, pacote e descrição de um objeto novo (só estrutura e tabela). Sob a trava de escrita.</summary>
    Task<Result<DdicNameValidation>> ValidateNameAsync(SapConnection connection, DdicKind kind, string name, string package, string description,
        CancellationToken ct);

    /// <summary>[Criação] → LOCK → (confere a request) → PUT → UNLOCK em finally → ativação stateless.</summary>
    Task<Result<DdicWriteOutcome>> WriteAsync(SapConnection connection, DdicWriteRequest request, CancellationToken ct);
}
