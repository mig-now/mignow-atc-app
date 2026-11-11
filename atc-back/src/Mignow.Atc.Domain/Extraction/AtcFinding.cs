using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Extraction;

/// <summary>
/// Finding de uma extração: campos indexados para filtro + <see cref="AllColumnsJson"/> com todas as colunas do CSV
/// total, na ordem (o export reproduz o CSV da skill). <see cref="Seq"/> = ordem da linha no CSV.
/// </summary>
public sealed class AtcFinding : EntityBase
{
    private AtcFinding() { }

    public int ExtractionId { get; private set; }

    public int Seq { get; private set; }

    public int? Priority { get; private set; }

    public string? CheckTitle { get; private set; }

    public string? MessageTitle { get; private set; }

    public string ObjectName { get; private set; } = string.Empty;

    public string? ObjectType { get; private set; }

    public string? Package { get; private set; }

    public string? RefObjectName { get; private set; }

    /// <summary>RObT.</summary>
    public string? RefObjectType { get; private set; }

    public string? Note { get; private set; }

    /// <summary><c>quickfixInfo</c>/markerId do finding (usado no pedido de isenção).</summary>
    public string? FindingId { get; private set; }

    public bool InBaseline { get; private set; }

    public string AllColumnsJson { get; private set; } = "{}";

    public static AtcFinding Create(int extractionId, int seq, int? priority, string? checkTitle, string? messageTitle, string objectName,
        string? objectType, string? package, string? refObjectName, string? refObjectType, string? note, string? findingId, bool inBaseline,
        string allColumnsJson) =>
        new()
        {
            ExtractionId = extractionId,
            Seq = seq,
            Priority = priority,
            CheckTitle = Guard.Optional(checkTitle, 300),
            MessageTitle = Guard.Optional(messageTitle, 500),
            ObjectName = Guard.Required(objectName, "Finding.ObjectRequired", 120),
            ObjectType = Guard.Optional(objectType, 10),
            Package = Guard.Optional(package, 60),
            RefObjectName = Guard.Optional(refObjectName, 200),
            RefObjectType = Guard.Optional(refObjectType, 10),
            Note = Guard.Optional(note, 40),
            FindingId = Guard.Optional(findingId, 300),
            InBaseline = inBaseline,
            AllColumnsJson = string.IsNullOrWhiteSpace(allColumnsJson) ? "{}" : allColumnsJson,
        };
}
