using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.RemediationApply;

public sealed class ApplyPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.Apply;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ApplyStep)];
}
