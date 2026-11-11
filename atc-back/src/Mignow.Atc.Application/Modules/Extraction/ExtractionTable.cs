namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Tabela da extração: colunas na ordem do CSV total e uma linha por finding (só as chaves presentes; o que falta é vazio,
/// como na hashtable do script). Comparação de chaves sem diferenciar maiúsculas (hashtable do PowerShell).
/// </summary>
public sealed record ExtractionTable(IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyDictionary<string, string>> Rows);
