namespace Mignow.Atc.Domain.Scan;

/// <summary>
/// O que a saída de um lote informa (linhas F/E/X/O/Z da classe) e como foi a execução: base das somas do resumo
/// (<c>lidos</c>) e da estatística original × refinada. Amostras = as 10 primeiras linhas E/X do lote.
/// </summary>
public sealed record ScanBatchStats(
    int ObjectsRead, int SourcesRead, int LinesRead, int Occurrences, int ObjectErrors, int RegexErrors, int ServerMs, int ScanErrors,
    int? TimeSeconds, string? Message, IReadOnlyList<string> Messages, IReadOnlyList<IReadOnlyList<string>> ObjectErrorSample,
    IReadOnlyList<IReadOnlyList<string>> RegexErrorSample, IReadOnlyDictionary<string, int> OriginalByRule, DateTimeOffset? StartedAt);
