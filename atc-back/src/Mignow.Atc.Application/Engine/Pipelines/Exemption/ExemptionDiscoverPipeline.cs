using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Exemption;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines.Exemption;

/// <summary>Pipeline do run <c>ExemptionDiscover</c> (um passo).</summary>
public sealed class ExemptionDiscoverPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ExemptionDiscover;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ExemptionDiscoverStep)];
}
