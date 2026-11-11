using Mignow.Atc.Application.Engine.Running;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;
using Mignow.Atc.Application.Features.Scans.GetP1File;
using Mignow.Atc.Application.Features.Sessions;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Scans.Import;

/// <summary>Importa o CSV P1 pelo ponto de entrada da remediação (<see cref="SessionCreator"/>): reimportar arquiva a sessão anterior do ambiente.</summary>
public sealed class ImportScanHandler(IDataStore store, RunStarter starter, SessionCreator creator, IStringLocalizer<Errors> l) : ICommandHandler<ImportScanCommand, Result<SessionResponse>>
{
    public async Task<Result<SessionResponse>> Handle(ImportScanCommand c, CancellationToken ct)
    {
        var scan = await ScanViews.LoadAsync(store, c.Id, false, ct);
        if (scan is null)
            return l.NotFound("Scan.NotFound");
        var p1 = await ScanP1Source.LoadAsync(store, l, scan, c.IncludeOtherLine, ct);
        if (p1.IsFailure)
            return p1.Error;
        if (p1.Value.Rows.Count == 0)
            return l.Conflict("Scan.NothingToImport");
        var system = await SessionViews.LoadSystemAsync(store, scan.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var document = new CsvDocument("utf-8-sig", ";", ScanP1Csv.Layout, p1.Value.Rows);
        var created = await creator.CreateSessionFromRowsAsync(system, new SessionSource(RemediationSource.MignowScan, p1.Value.FileName, "1", null, document), ct);
        if (created.IsFailure)
            return created.Error;
        var response = (await SessionViews.BuildAsync(store, [created.Value.Session], ct, created.Value.Summary, created.Value.CarriedOver))[0];
        return await SessionSourcesAutoRead.StartAsync(starter, system, created.Value.Session, response, ct);
    }
}
