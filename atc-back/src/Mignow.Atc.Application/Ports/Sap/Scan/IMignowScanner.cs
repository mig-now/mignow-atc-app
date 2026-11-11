using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap.Scan;

/// <summary>
/// Porta SAP da varredura MIGNOW (skill atc-extracao-mignow). Leitura: inventário da TADIR e simulação. Escrita (única da
/// skill): classe temporária em <c>$TMP</c> sem request — criada, executada (classrun) e apagada; toda escrita começa
/// pela trava de produção do núcleo. Nunca altera os objetos encontrados.
/// </summary>
public interface IMignowScanner
{
    /// <summary>Porte de <c>inventario.ps1</c> (leitura): TADIR R3TR dos tipos pedidos com nome ou pacote Z*/Y*.</summary>
    Task<Result<IReadOnlyList<ScanTadirRow>>> InventoryAsync(SapConnection connection, IReadOnlyList<string> types, CancellationToken ct);

    /// <summary>Porte de <c>executar.ps1 -Simular</c> (leitura): T000, nomes livres e validação no ADT.</summary>
    Task<Result<ScanSimulation>> SimulateAsync(SapConnection connection, IReadOnlyList<string> classNames, CancellationToken ct);

    /// <summary>
    /// Porte de <c>executar.ps1 -Confirmado</c> para UM processo: cria a classe, por lote grava, ativa e roda (classrun) e
    /// chama <paramref name="onLot"/> (devolve <c>false</c> para parar num ponto seguro); apaga a classe SEMPRE no final.
    /// Para no primeiro lote com erro, como o script.
    /// </summary>
    Task<Result<ScanProcessResult>> ExecuteProcessAsync(
        SapConnection connection, ScanProcessRequest request, Func<ScanLotResult, Task<bool>> onLot, CancellationToken ct);

    /// <summary>Porte de <c>-SoRemover</c>: apaga as classes temporárias que ficaram no sistema (só se estiverem em <c>$TMP</c>).</summary>
    Task<Result<IReadOnlyList<ScanClassRemoval>>> RemoveResiduesAsync(SapConnection connection, IReadOnlyList<string> classNames, CancellationToken ct);
}
