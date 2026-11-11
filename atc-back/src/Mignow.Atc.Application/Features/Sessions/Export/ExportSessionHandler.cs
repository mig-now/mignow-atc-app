using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Export;

public sealed class ExportSessionHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<ExportSessionQuery, Result<ExportedCsv>>
{
    public async Task<Result<ExportedCsv>> Handle(ExportSessionQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var id = session.Id;
        var items = await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == id).OrderBy(i => i.N), ct);
        return SessionExporter.Export(session, items);
    }
}
