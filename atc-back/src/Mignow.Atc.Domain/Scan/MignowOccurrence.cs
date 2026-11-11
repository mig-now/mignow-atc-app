using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Scan;

/// <summary>Ocorrência de uma regra MIGNOW num fonte. <see cref="Class"/> = R / I / pendente (classificação da análise).</summary>
public sealed class MignowOccurrence : EntityBase
{
    private MignowOccurrence() { }

    public int ScanId { get; private set; }

    public string RuleId { get; private set; } = string.Empty;

    public string ObjectName { get; private set; } = string.Empty;

    public string ObjectType { get; private set; } = string.Empty;

    public string? Include { get; private set; }

    public string? AdtSource { get; private set; }

    public int Line { get; private set; }

    public int? LineEnd { get; private set; }

    public string? Snippet { get; private set; }

    public string? Cross { get; private set; }

    /// <summary>Situação: P pendente, R já remediada, I informativa (<see cref="ScanOccurrenceStatus"/>).</summary>
    public string? Class { get; private set; }

    /// <summary>Elemento (Referenced Object do layout da remediação): tabela, BAPI, transação…</summary>
    public string Element { get; private set; } = string.Empty;

    /// <summary>Lote que devolveu a ocorrência (ordem de leitura do cruzamento).</summary>
    public int Batch { get; private set; }

    public static MignowOccurrence Create(int scanId, string ruleId, string objectName, string objectType, string? include, string? adtSource,
        int line, int? lineEnd, string? snippet, string element = "", int batch = 0, string? @class = null) =>
        new()
        {
            ScanId = scanId,
            RuleId = Guard.Required(ruleId, "Occurrence.RuleRequired", 60),
            ObjectName = Guard.Required(objectName, "Occurrence.ObjectRequired", 120),
            ObjectType = Guard.Required(objectType, "Occurrence.ObjectTypeRequired", 10),
            Include = Guard.Optional(include, 200),
            AdtSource = Guard.Optional(adtSource, 500),
            Line = line,
            LineEnd = lineEnd,
            Snippet = snippet,
            Element = (element ?? string.Empty).Length > 200 ? element![..200] : element ?? string.Empty,
            Batch = batch,
            Class = Guard.Optional(@class, 20),
        };

    public void SetCross(string cross)
    {
        if (!ScanCross.All.Contains(cross, StringComparer.Ordinal))
            throw new DomainException("Occurrence.InvalidCross", $"Cruzamento inválido: {cross}.");
        Cross = cross;
    }

    public void Classify(string? @class) => Class = Guard.Optional(@class, 20);
}
