using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;

namespace Mignow.Atc.Application.Features.Exemptions.Simulate;

/// <summary>Enfileira o run <c>ExemptionSimulate</c> do lote (202); <c>Seq</c> = só esses findings (motivos diferentes no mesmo lote: um run por motivo).</summary>
public sealed record StartExemptionSimulateCommand(Guid BatchId, IReadOnlyList<int>? Seq = null) : ICommand<Result<RunResponse>>;
