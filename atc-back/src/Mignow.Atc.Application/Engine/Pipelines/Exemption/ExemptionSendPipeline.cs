using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Exemption;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines.Exemption;

/// <summary>Pipeline do run <c>ExemptionSend</c> (um passo).</summary>
public sealed class ExemptionSendPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ExemptionSend;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ExemptionSendStep)];
}
