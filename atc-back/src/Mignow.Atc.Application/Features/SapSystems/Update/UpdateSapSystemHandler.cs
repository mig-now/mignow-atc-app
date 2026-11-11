using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.Update;

public sealed class UpdateSapSystemHandler(IDataStore store, IStringLocalizer<Errors> l) : ICommandHandler<UpdateSapSystemCommand, Result<SapSystemResponse>>
{
    public async Task<Result<SapSystemResponse>> Handle(UpdateSapSystemCommand c, CancellationToken ct)
    {
        var system = await SapSystemLoader.LoadAsync(store, c.Id, true, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        var sid = c.Sid.Trim().ToUpper();
        var client = c.Client.Trim();
        if (await store.AnyAsync(store.Read<SapSystem>().Where(s => s.CustomerId == system.CustomerId && s.Sid == sid && s.Client == client && s.Id != system.Id), ct))
            return l.Conflict("SapSystem.AlreadyExists", sid, client);
        system.Update(c.Sid, c.Client, c.Host, c.AdtPort, c.Https, c.Language, c.CertThumbprint, c.IsProduction, c.ProjectType, c.Author,
            c.DefaultRequest, c.ExemptionApprover, c.Username, c.Active);
        return system.ToResponse();
    }
}
