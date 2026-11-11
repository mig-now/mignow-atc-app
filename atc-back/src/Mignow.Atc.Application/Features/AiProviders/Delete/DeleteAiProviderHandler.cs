using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Application.Ports.Secrets;
namespace Mignow.Atc.Application.Features.AiProviders.Delete;

public sealed class DeleteAiProviderHandler(IDataStore store, IStringLocalizer<Errors> l, ISecretStore secrets) : ICommandHandler<DeleteAiProviderCommand, Result>
{
    public async Task<Result> Handle(DeleteAiProviderCommand command, CancellationToken ct)
    {
        var provider = await store.FirstOrDefaultAsync(store.Query<AiProvider>().Where(p => p.Guid == command.Id), ct);
        if (provider is null)
            return Result.Failure(l.NotFound("AiProvider.NotFound"));
        if (provider.IsDefault)
            return Result.Failure(l.Conflict("AiProvider.IsDefault"));
        if (provider.ApiKeySecretRef is not null)
            await secrets.DeleteAsync(provider.ApiKeySecretRef, ct);
        store.Remove(provider);
        return Result.Success();
    }
}
