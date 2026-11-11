using Mignow.Atc.Application.Engine.Remediation;
using Mignow.Atc.Application.Engine.RemediationApply;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>
/// Remediação automática (o "play" da sessão): fontes → triagem (R1..R9 + produtos) → conferência da R1 no DDIC → proposta pela KB → proposta pela
/// IA do que ficou sem correção → confirmação automática (KB sem alerta; IA com sintaxe ok e sem alerta) → simulação com backup → revisão por IA →
/// aplicação (a revisão que bloqueia segura o item) → relatório do lote → resumo. Objetos standard ficam fora (consultor). Nada para no meio por
/// item: o que não passa nas travas fica pendente para o consultor, como no modo sem confirmação da skill.
/// </summary>
public sealed class AutoRemediationPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.AutoRemediation;

    public IReadOnlyList<Type> StepTypes { get; } =
    [
        typeof(AutoSourcesPlanStep), typeof(ReadSourcesFetchStep), typeof(AutoTriageStep), typeof(ConfirmR1CheckStep), typeof(ConfirmR1ApplyStep),
        typeof(ProposeStep), typeof(AutoProposeAiStep), typeof(AutoRemediationConfirmStep), typeof(AutoSimulateStep), typeof(AutoReviewStep),
        typeof(AutoApplyStep), typeof(BatchReportStep), typeof(AutoRemediationSummaryStep),
    ];
}
