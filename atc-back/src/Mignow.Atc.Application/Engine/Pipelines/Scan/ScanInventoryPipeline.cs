using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Scan;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines.Scan;

public sealed class ScanInventoryPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ScanInventory;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ScanInventoryStep)];
}
