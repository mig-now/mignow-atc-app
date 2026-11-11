using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.AiProviders.Update;

public sealed record UpdateAiProviderCommand(Guid Id, string Name, AiProviderKind Kind, string Model, string? ApiKey, bool IsDefault, bool Active)
    : ICommand<Result<AiProviderResponse>>, IAiProviderInput;
