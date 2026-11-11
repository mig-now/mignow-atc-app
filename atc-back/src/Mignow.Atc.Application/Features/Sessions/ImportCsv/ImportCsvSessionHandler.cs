using Mignow.Atc.Application.Engine.Running;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.SapSystems;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.ImportCsv;

public sealed class ImportCsvSessionHandler(IDataStore store, RunStarter starter, SessionCreator creator, IStringLocalizer<Errors> l)
    : ICommandHandler<ImportCsvSessionCommand, Result<SessionResponse>>
{
    public async Task<Result<SessionResponse>> Handle(ImportCsvSessionCommand command, CancellationToken ct)
    {
        var system = await SapSystemLoader.LoadAsync(store, command.SapSystemId, false, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var doc = AtcCsv.Read(command.Content);
        if (doc is null)
            return l.Validation("Session.CsvEmpty");
        var created = await creator.CreateSessionFromRowsAsync(system,
            new SessionSource(RemediationSource.CsvImport, Path.GetFileName(command.FileName), command.Priorities, command.Request, doc), ct);
        if (created.IsFailure)
            return created.Error;
        var response = (await SessionViews.BuildAsync(store, [created.Value.Session], ct, created.Value.Summary, created.Value.CarriedOver))[0];
        return await SessionSourcesAutoRead.StartAsync(starter, system, created.Value.Session, response, ct);
    }
}
