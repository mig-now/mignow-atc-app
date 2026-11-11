using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;

namespace Mignow.Atc.Application.Features.AiProviders.List;

public sealed record ListAiProvidersQuery : IQuery<Result<IReadOnlyList<AiProviderResponse>>>;
