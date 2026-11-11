using System.Text.Json;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Dtos.Runs;

public static class RunMappings
{
    public static IReadOnlyDictionary<string, string?>? Args(string? json) =>
        json is null ? null : Json.Read<Dictionary<string, string?>>(json, []);

    public static JsonElement? Element(string? json)
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

    public static RunStepResponse ToResponse(this RunStep s) =>
        new(s.Code, s.Order, s.Status, s.StartedAt, s.FinishedAt, s.MessageCode, Args(s.MessageArgs), s.ProgressDone, s.ProgressTotal);
}
