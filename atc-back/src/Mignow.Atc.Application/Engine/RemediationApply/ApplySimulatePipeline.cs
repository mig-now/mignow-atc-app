using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.RemediationApply;

public sealed class ApplySimulatePipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ApplySimulate;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(SimulateStep), typeof(ReviewStep)];
}
