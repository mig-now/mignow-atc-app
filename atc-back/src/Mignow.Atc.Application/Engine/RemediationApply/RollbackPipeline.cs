using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.RemediationApply;

public sealed class RollbackPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.Rollback;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(RollbackStep)];
}
