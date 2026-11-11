namespace Mignow.Atc.Application.Ports.Sap.Extraction;

/// <summary>Item ATC (<c>SATC_AC_ITM</c>).</summary>
public sealed record AtcItem(string Type, string CreatedOn, string UpdatedOn, string LastRunIndex, string Processor, string Status);

/// <summary>Entrada de baseline (<c>SATC_AC_BSI</c>) de um finding (pode haver mais de uma).</summary>
public sealed record AtcBaselineEntry(string BaselineIx, string BaselineEffect);

/// <summary>Autor e pacote do objeto (<c>TADIR</c>).</summary>
public sealed record AtcTadirEntry(string Author, string DevClass);

/// <summary>
/// Complementos lidos por SQL (todos os dicionários sem diferenciar maiúsculas, como as hashtables do PowerShell):
/// itens por <c>ITEM_ID</c> (maiúsculo), baselines por <c>ITEM_ID</c>, TADIR por <c>OBJECT|OBJ_NAME</c>, títulos de nota por
/// <c>NUMM</c> e textos de domínio por valor. <paramref name="MissingTables"/>: tabelas opcionais que não existem no sistema
/// (<c>SATC_AC_BSI</c>, <c>CWBNTSTXT</c>); sem elas a extração segue sem baseline / sem título de nota.
/// </summary>
public sealed record AtcComplements(
    IReadOnlyDictionary<string, AtcItem> Items,
    IReadOnlyDictionary<string, string> StatusTexts,
    IReadOnlyDictionary<string, string> TypeTexts,
    IReadOnlyDictionary<string, IReadOnlyList<AtcBaselineEntry>> Baselines,
    IReadOnlyDictionary<string, string> EffectTexts,
    IReadOnlyDictionary<string, AtcTadirEntry> Tadir,
    IReadOnlyDictionary<string, string> NoteTitles,
    IReadOnlyDictionary<string, string> CategoryTexts,
    IReadOnlyDictionary<string, string> ExemptionTexts,
    IReadOnlyList<string>? MissingTables = null);
