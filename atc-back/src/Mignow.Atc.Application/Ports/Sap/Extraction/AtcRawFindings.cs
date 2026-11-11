namespace Mignow.Atc.Application.Ports.Sap.Extraction;

/// <summary>
/// Findings lidos do ADT, uma linha por finding: <c>obj_*</c> (atributos do objeto), atributos do finding, <c>item_id</c>,
/// <c>documentation_link</c>, <c>quickfix_*</c> e <c>tag_*</c>. As três listas de colunas guardam a ORDEM de descoberta
/// (tags em ordem alfabética), que define a ordem das colunas do CSV total.
/// </summary>
public sealed record AtcRawFindings(
    IReadOnlyList<string> ObjectColumns, IReadOnlyList<string> FindingColumns, IReadOnlyList<string> TagColumns,
    IReadOnlyList<IReadOnlyDictionary<string, string>> Rows);
