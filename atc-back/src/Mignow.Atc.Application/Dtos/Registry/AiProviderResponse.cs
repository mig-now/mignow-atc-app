using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Dtos.Registry;

/// <summary>Provedor de IA. A chave nunca volta: só <see cref="HasApiKey"/>.</summary>
public sealed record AiProviderResponse(Guid Id, string Name, AiProviderKind Kind, string Model, bool HasApiKey, bool IsDefault, bool Active);
