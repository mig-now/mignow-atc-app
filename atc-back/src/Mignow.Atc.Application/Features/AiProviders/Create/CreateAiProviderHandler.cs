using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.AiProviders.Create;

public sealed class CreateAiProviderHandler(IDataStore store, ISecretStore secrets)
    : ICommandHandler<CreateAiProviderCommand, Result<AiProviderResponse>>
{
    public async Task<Result<AiProviderResponse>> Handle(CreateAiProviderCommand c, CancellationToken ct)
    {
        var provider = AiProvider.Create(c.Name, c.Kind, c.Model);
        if (!string.IsNullOrWhiteSpace(c.ApiKey))
            provider.SetApiKeySecret(await secrets.SetAsync(null, $"ai:{provider.Guid}", c.ApiKey.Trim(), ct));
        store.Add(provider);
        await AiProviderDefaults.ApplyAsync(store, provider, c.IsDefault, ct);
        return provider.ToResponse();
    }
}
