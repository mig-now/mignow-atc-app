using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Extraction;

/// <summary>
/// Extração de uma execução concluída do ATC de série <c>ZMIG_</c>/<c>Z_MIG_</c> (skill atc-extracao). Contagens por
/// prioridade no total e sem baseline; <see cref="VariationJson"/> = variação contra a extração anterior do ambiente.
/// </summary>
public sealed class AtcExtraction : EntityBase
{
    private AtcExtraction() { }

    public int SapSystemId { get; private set; }

    public string Series { get; private set; } = string.Empty;

    /// <summary>EXEC_ID / displayId da execução do ATC.</summary>
    public string DisplayId { get; private set; } = string.Empty;

    public string? Title { get; private set; }

    public DateTimeOffset? ExecutedAtUtc { get; private set; }

    public DateTimeOffset ExtractedAt { get; private set; }

    public int TotalCount { get; private set; }

    public int InBaselineCount { get; private set; }

    public int P1Total { get; private set; }

    public int P2Total { get; private set; }

    public int P3Total { get; private set; }

    public int P1New { get; private set; }

    public int P2New { get; private set; }

    public int P3New { get; private set; }

    public string? VariationJson { get; private set; }

    /// <summary>
    /// Nomes das colunas do CSV total, NA ORDEM (o jsonb do Postgres não preserva a ordem das chaves de
    /// <see cref="AtcFinding.AllColumnsJson"/>). Lista JSON de strings.
    /// </summary>
    public string ColumnsJson { get; private set; } = "[]";

    /// <summary>Dados da execução do ATC (variante, agendada por, prioridades do cabeçalho, em andamento…): JSON livre.</summary>
    public string? ExecutionJson { get; private set; }

    public ExtractionSituation Situation { get; private set; }

    /// <summary>
    /// Só vale (listagem, "já extraída", exportação) depois de todos os findings gravados: a extração é gravada em lotes e a
    /// que ficou pela metade (queda) é apagada e refeita no retry do passo.
    /// </summary>
    public bool IsComplete { get; private set; }

    public static AtcExtraction Create(int sapSystemId, string series, string displayId, string? title, DateTimeOffset? executedAtUtc,
        DateTimeOffset extractedAt) =>
        new()
        {
            SapSystemId = sapSystemId,
            Series = Guard.Required(series, "Extraction.SeriesRequired", 120),
            DisplayId = Guard.Required(displayId, "Extraction.DisplayIdRequired", 120),
            Title = Guard.Optional(title, 300),
            ExecutedAtUtc = executedAtUtc,
            ExtractedAt = extractedAt,
            Situation = ExtractionSituation.Extracted,
        };

    public void SetCounts(int total, int inBaseline, int p1Total, int p2Total, int p3Total, int p1New, int p2New, int p3New)
    {
        if (new[] { total, inBaseline, p1Total, p2Total, p3Total, p1New, p2New, p3New }.Any(n => n < 0))
            throw new DomainException("Extraction.InvalidCounts", "Contagens negativas.");
        TotalCount = total;
        InBaselineCount = inBaseline;
        P1Total = p1Total;
        P2Total = p2Total;
        P3Total = p3Total;
        P1New = p1New;
        P2New = p2New;
        P3New = p3New;
    }

    public void MarkComplete() => IsComplete = true;

    public void SetVariation(string? variationJson) => VariationJson = variationJson;

    public void SetColumns(string columnsJson) => ColumnsJson = string.IsNullOrWhiteSpace(columnsJson) ? "[]" : columnsJson;

    public void SetExecution(string? executionJson) => ExecutionJson = executionJson;

    public void MarkAlreadyExtracted() => Situation = ExtractionSituation.AlreadyExtracted;
}
