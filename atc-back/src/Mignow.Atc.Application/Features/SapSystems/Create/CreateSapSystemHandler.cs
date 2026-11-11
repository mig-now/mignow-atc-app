using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.Create;

public sealed class CreateSapSystemHandler(IDataStore store, ISecretStore secrets, IStringLocalizer<Errors> l)
    : ICommandHandler<CreateSapSystemCommand, Result<SapSystemResponse>>
{
    public async Task<Result<SapSystemResponse>> Handle(CreateSapSystemCommand c, CancellationToken ct)
    {
        var customer = await store.FirstOrDefaultAsync(store.Query<Customer>().Where(x => x.Guid == c.CustomerId), ct);
        if (customer is null)
            return l.NotFound("Customer.NotFound");
        var sid = c.Sid.Trim().ToUpper();
        var client = c.Client.Trim();
        if (await store.AnyAsync(store.Read<SapSystem>().Where(s => s.CustomerId == customer.Id && s.Sid == sid && s.Client == client), ct))
            return l.Conflict("SapSystem.AlreadyExists", sid, client);
        var system = SapSystem.Create(customer, c.Sid, c.Client, c.Host, c.AdtPort, c.Https, c.Language, c.CertThumbprint, c.IsProduction,
            c.ProjectType, c.Author, c.DefaultRequest, c.ExemptionApprover, c.Username);
        if (!string.IsNullOrEmpty(c.Password))
            system.SetPasswordSecret(await secrets.SetAsync(null, $"sap:{system.Guid}", c.Password, ct));
        store.Add(system);
        return system.ToResponse();
    }
}
