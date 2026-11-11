using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Scans.GetSummaryFile;

public sealed class GetSummaryFileHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<GetSummaryFileQuery, Result<ScanFileResponse>>
{
    public async Task<Result<ScanFileResponse>> Handle(GetSummaryFileQuery query, CancellationToken ct)
    {
        var scan = await ScanViews.LoadAsync(store, query.Id, false, ct);
        if (scan is null)
            return l.NotFound("Scan.NotFound");
        if (scan.SummaryJson is null || JsonNode.Parse(scan.SummaryJson) is not JsonObject summary)
            return l.Conflict("Scan.NotCrossed");
        return new ScanFileResponse("resumo.md", "text/markdown; charset=utf-8", new UTF8Encoding(false).GetBytes(ScanSummaryMarkdown.Build(summary)));
    }
}
