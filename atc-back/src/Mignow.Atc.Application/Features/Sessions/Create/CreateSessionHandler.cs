using Mignow.Atc.Application.Engine.Running;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Create;

public sealed class CreateSessionHandler(IDataStore store, RunStarter starter, SessionCreator creator, IStringLocalizer<Errors> l)
    : ICommandHandler<CreateSessionCommand, Result<SessionResponse>>
{
    public async Task<Result<SessionResponse>> Handle(CreateSessionCommand command, CancellationToken ct)
    {
        var extraction = await store.FirstOrDefaultAsync(store.Read<AtcExtraction>().Where(e => e.Guid == command.ExtractionId), ct);
        if (extraction is null)
            return l.NotFound("Extraction.NotFound");
        var system = await SessionViews.LoadSystemAsync(store, extraction.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var findings = await store.ListAsync(store.Read<AtcFinding>().Where(f => f.ExtractionId == extraction.Id && !f.InBaseline), ct);
        var created = await creator.CreateSessionFromRowsAsync(system,
            new SessionSource(RemediationSource.Extraction, extraction.DisplayId, command.Priorities, command.Request, ExtractionRows.ToDocument(findings)), ct);
        if (created.IsFailure)
            return created.Error;
        var response = (await SessionViews.BuildAsync(store, [created.Value.Session], ct, created.Value.Summary, created.Value.CarriedOver))[0];
        return await SessionSourcesAutoRead.StartAsync(starter, system, created.Value.Session, response, ct);
    }
}
