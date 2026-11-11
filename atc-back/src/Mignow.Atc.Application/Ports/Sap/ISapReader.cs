using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// Leitura do SAP via ADT (stateless). Porte de <c>testar_conexao</c>, <c>ler_fonte</c> e <c>verificar_sintaxe</c>.
/// As leituras de módulo (DDIC, resultado do ATC…) ficam em portas próprias em <c>Ports/Sap/&lt;Módulo&gt;/</c>.
/// </summary>
public interface ISapReader
{
    /// <summary>Discovery (credencial + VPN), release e categoria do mandante (T000-CCCATEGORY).</summary>
    Task<Result<SapSystemInfo>> TestConnectionAsync(SapConnection connection, CancellationToken ct);

    /// <summary>Busca rápida no repositório (quickSearch); <paramref name="adtType"/> opcional (ex.: FUGR/FF).</summary>
    Task<Result<IReadOnlyList<SapObjectRef>>> SearchAsync(SapConnection connection, string query, string? adtType, int maxResults, CancellationToken ct);

    /// <summary>Fontes físicos do objeto (programa + includes do objectstructure; classe = main + includes locais).</summary>
    Task<Result<IReadOnlyList<SapSourcePart>>> GetObjectSourcesAsync(SapConnection connection, string name, string type, CancellationToken ct);

    /// <summary>Texto do fonte. <paramref name="activeVersion"/> = <c>?version=active</c> (sem ele vem a inativa, se houver).</summary>
    Task<Result<string>> GetSourceAsync(SapConnection connection, string sourceUri, bool activeVersion, CancellationToken ct);

    /// <summary>Includes/partes do objeto pelo <c>objectstructure</c>.</summary>
    Task<Result<IReadOnlyList<SapObjectRef>>> GetStructureAsync(SapConnection connection, string objectUri, CancellationToken ct);

    /// <summary>Verificação de sintaxe da versão ativa (checkruns). Todas as severidades.</summary>
    Task<Result<IReadOnlyList<SyntaxMessage>>> CheckSyntaxAsync(SapConnection connection, string objectUri, CancellationToken ct);

    /// <summary>Verifica um fonte ainda não gravado (conteúdo no corpo; nada muda no SAP). Include: com o contexto.</summary>
    Task<Result<IReadOnlyList<SyntaxMessage>>> CheckSourceAsync(SapConnection connection, string objectUri, string source, string? contextUri,
        CancellationToken ct);
}
