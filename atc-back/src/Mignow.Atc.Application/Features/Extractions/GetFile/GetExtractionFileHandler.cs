using System.Text.Json;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Extractions.GetFile;

public sealed class GetExtractionFileHandler(IDataStore store, IStringLocalizer<Errors> l)
    : IQueryHandler<GetExtractionFileQuery, Result<ExtractionFileResponse>>
{
    public async Task<Result<ExtractionFileResponse>> Handle(GetExtractionFileQuery query, CancellationToken ct)
    {
        var extraction = await store.FirstOrDefaultAsync(store.Read<AtcExtraction>().Where(e => e.Guid == query.ExtractionId && e.IsComplete), ct);
        if (extraction is null)
            return l.NotFound("Extraction.NotFound");
        var system = (await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(s => s.Id == extraction.SapSystemId), ct))!;
        var priorities = ExtractionFiles.ParsePriorities(query.Priorities) ?? [];
        var id = extraction.Id;
        var findings = store.Read<AtcFinding>().Where(f => f.ExtractionId == id);
        if (query.Kind != ExtractionFileKind.Total)
            findings = findings.Where(f => !f.InBaseline);
        if (priorities.Count > 0)
        {
            var wanted = priorities.Select(p => (int?)p).ToArray();
            findings = findings.Where(f => wanted.Contains(f.Priority));
        }
        var rows = (await store.ListAsync(findings.OrderBy(f => f.Seq).Select(f => f.AllColumnsJson), ct)).Select(ExtractionFiles.ParseRow).ToList();
        var columns = Json.Read<List<string>>(extraction.ColumnsJson, []);
        var baseName = ExtractionNaming.BaseName(system.Sid, system.Client, extraction.Series, extraction.ExecutedAtUtc, extraction.DisplayId);
        return new ExtractionFileResponse(ExtractionFiles.FileName(baseName, query.Kind, priorities), ExtractionFiles.Content(query.Kind, columns, rows));
    }
}
