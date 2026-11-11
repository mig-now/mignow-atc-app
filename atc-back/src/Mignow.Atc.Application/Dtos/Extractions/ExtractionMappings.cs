using System.Text.Json;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Dtos.Extractions;

public static class ExtractionMappings
{
    public static ExtractionResponse ToResponse(this AtcExtraction e, SapSystem system) =>
        new(e.Guid, system.Guid, e.Series, e.DisplayId, e.Title, e.ExecutedAtUtc, e.ExtractedAt, e.TotalCount, e.InBaselineCount, e.TotalCount - e.InBaselineCount,
            new PriorityCounts(e.P1Total, e.P2Total, e.P3Total), new PriorityCounts(e.P1New, e.P2New, e.P3New), Element(e.VariationJson),
            Element(e.ExecutionJson), e.Situation, BaseName(e, system));

    public static string BaseName(AtcExtraction e, SapSystem system) =>
        ExtractionNaming.BaseName(system.Sid, system.Client, e.Series, e.ExecutedAtUtc, e.DisplayId);

    public static FindingResponse ToResponse(this AtcFinding f, bool includeColumns) =>
        new(f.Guid, f.Seq, f.Priority, f.CheckTitle, f.MessageTitle, f.ObjectName, f.ObjectType, f.Package, f.RefObjectName, f.RefObjectType, f.Note, f.FindingId,
            f.InBaseline, includeColumns ? Element(f.AllColumnsJson) : null);

    private static JsonElement? Element(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
