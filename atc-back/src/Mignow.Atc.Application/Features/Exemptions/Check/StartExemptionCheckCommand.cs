using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Exemptions.Check;

/// <summary>Enfileira o run <c>ExemptionCheck</c> do lote (202).</summary>
public sealed record StartExemptionCheckCommand(Guid BatchId) : ICommand<Result<RunResponse>>;
