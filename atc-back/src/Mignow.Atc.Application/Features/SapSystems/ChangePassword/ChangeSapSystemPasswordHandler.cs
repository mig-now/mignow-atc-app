using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.ChangePassword;

public sealed class ChangeSapSystemPasswordHandler(IDataStore store, ISecretStore secrets, IStringLocalizer<Errors> l)
    : ICommandHandler<ChangeSapSystemPasswordCommand, Result>
{
    public async Task<Result> Handle(ChangeSapSystemPasswordCommand command, CancellationToken ct)
    {
        var system = await store.FirstOrDefaultAsync(store.Query<SapSystem>().Where(s => s.Guid == command.Id), ct);
        if (system is null)
            return Result.Failure(l.NotFound("SapSystem.NotFound"));
        system.SetPasswordSecret(await secrets.SetAsync(system.PasswordSecretRef, $"sap:{system.Guid}", command.Password, ct));
        return Result.Success();
    }
}
