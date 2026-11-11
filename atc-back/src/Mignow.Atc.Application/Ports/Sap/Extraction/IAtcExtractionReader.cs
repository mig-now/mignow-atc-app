using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap.Extraction;

/// <summary>
/// Leitura do resultado do ATC para a extração (porte de <c>extrair.ps1</c> + <c>AtcExtracao.psm1</c>). Somente leitura:
/// ADT GET e SQL por data preview STATELESS. A regra de escolha da execução e a montagem das colunas ficam na Application
/// (<c>Modules/Extraction</c>); a porta só traz os dados do SAP.
/// </summary>
public interface IAtcExtractionReader
{
    /// <summary>
    /// Cabeçalhos (<c>SATC_AC_RESULTH</c>) das séries cujo nome casa com os prefixos (busca ampla com LIKE; o filtro exato do
    /// prefixo, <c>DISPLAY_ID = EXEC_ID</c> e <c>IS_REWORKED</c> é da Application).
    /// </summary>
    Task<Result<IReadOnlyList<AtcRunHeader>>> ListRunsAsync(SapConnection connection, IReadOnlyList<string> prefixes, CancellationToken ct);

    /// <summary>
    /// Findings da execução: <c>GET /sap/bc/adt/atc/results/{displayId}?includeExemptedFindings=true&amp;sap-language=&lt;L&gt;</c>
    /// (todos os atributos, tags e quickfix devolvidos, com os isentos).
    /// </summary>
    Task<Result<AtcRawFindings>> ReadFindingsAsync(SapConnection connection, string displayId, string language, CancellationToken ct);

    /// <summary>Complementos por SQL: item ATC, baseline, TADIR, título da nota e textos de domínio.</summary>
    Task<Result<AtcComplements>> ReadComplementsAsync(SapConnection connection, AtcComplementRequest request, CancellationToken ct);
}
