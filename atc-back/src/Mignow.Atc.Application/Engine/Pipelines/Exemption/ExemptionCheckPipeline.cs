using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Exemption;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines.Exemption;

/// <summary>Pipeline do run <c>ExemptionCheck</c> (um passo).</summary>
public sealed class ExemptionCheckPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ExemptionCheck;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ExemptionCheckStep)];
}
