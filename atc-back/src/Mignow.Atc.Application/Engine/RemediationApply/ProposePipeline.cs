using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Proposta; com <c>AutoConfirm</c> (modo sem confirmação) encadeia confirmação da KB sem alerta, simulação, revisão, aplicação e o relatório do lote.</summary>
public sealed class ProposePipeline : IRunPipeline
{
    public RunKind Kind => RunKind.Propose;

    public IReadOnlyList<Type> StepTypes { get; } =
        [typeof(ProposeStep), typeof(AutoConfirmStep), typeof(AutoSimulateStep), typeof(AutoReviewStep), typeof(AutoApplyStep), typeof(BatchReportStep)];
}
