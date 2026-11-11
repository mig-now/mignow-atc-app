using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Api.Contracts;

/// <summary><see cref="ApiKey"/> nulo/vazio = manter a atual.</summary>
public sealed record UpdateAiProviderRequest(string Name, AiProviderKind Kind, string Model, string? ApiKey, bool IsDefault, bool Active);
