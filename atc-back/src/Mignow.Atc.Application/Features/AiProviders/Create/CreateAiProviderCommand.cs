using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.AiProviders.Create;

public sealed record CreateAiProviderCommand(string Name, AiProviderKind Kind, string Model, string? ApiKey, bool IsDefault)
    : ICommand<Result<AiProviderResponse>>, IAiProviderInput;
