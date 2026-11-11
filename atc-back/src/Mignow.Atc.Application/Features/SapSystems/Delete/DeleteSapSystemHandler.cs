using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems.Delete;

public sealed class DeleteSapSystemHandler(IDataStore store, IStringLocalizer<Errors> l, ISecretStore secrets) : ICommandHandler<DeleteSapSystemCommand, Result>
{
    public async Task<Result> Handle(DeleteSapSystemCommand command, CancellationToken ct)
    {
        var system = await store.FirstOrDefaultAsync(store.Query<SapSystem>().Where(s => s.Guid == command.Id), ct);
        if (system is null)
            return Result.Failure(l.NotFound("SapSystem.NotFound"));
        var id = system.Id;
        if (await store.AnyAsync(store.Read<Run>().Where(r => r.SapSystemId == id), ct))
            return Result.Failure(l.Conflict("SapSystem.HasRuns"));
        if (system.PasswordSecretRef is not null)
            await secrets.DeleteAsync(system.PasswordSecretRef, ct);
        store.Remove(system);
        return Result.Success();
    }
}
