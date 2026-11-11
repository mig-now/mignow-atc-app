using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.AiProviders.Update;

public sealed class UpdateAiProviderHandler(IDataStore store, ISecretStore secrets, IStringLocalizer<Errors> l)
    : ICommandHandler<UpdateAiProviderCommand, Result<AiProviderResponse>>
{
    public async Task<Result<AiProviderResponse>> Handle(UpdateAiProviderCommand c, CancellationToken ct)
    {
        var provider = await store.FirstOrDefaultAsync(store.Query<AiProvider>().Where(p => p.Guid == c.Id), ct);
        if (provider is null)
            return l.NotFound("AiProvider.NotFound");
        provider.Update(c.Name, c.Kind, c.Model, c.Active);
        if (!string.IsNullOrWhiteSpace(c.ApiKey))
            provider.SetApiKeySecret(await secrets.SetAsync(provider.ApiKeySecretRef, $"ai:{provider.Guid}", c.ApiKey.Trim(), ct));
        await AiProviderDefaults.ApplyAsync(store, provider, c.IsDefault && c.Active, ct);
        return provider.ToResponse();
    }
}
