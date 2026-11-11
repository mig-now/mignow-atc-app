using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>Conferência da R1 no dicionário: consulta o DDIC e depois grava Isenção / volta a pendente.</summary>
public sealed class ConfirmR1Pipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ConfirmR1;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ConfirmR1CheckStep), typeof(ConfirmR1ApplyStep)];
}
