using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Exemptions.Discover;

/// <summary>Enfileira o run <c>ExemptionDiscover</c> do lote (202).</summary>
public sealed record StartExemptionDiscoverCommand(Guid BatchId) : ICommand<Result<RunResponse>>;
