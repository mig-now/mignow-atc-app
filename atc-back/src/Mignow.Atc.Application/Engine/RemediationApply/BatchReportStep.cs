using System.Text.Json;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation.Apply;

namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Modo sem confirmação: relatório do lote (markdown) no <c>ResultJson</c> do run, junto com o resumo da proposta e as aplicações.</summary>
public sealed class BatchReportStep : IRunStep
{
    public string Code => RemediationApplyStepCodes.BatchReport;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ProposeParameters>();
        if (!p.AutoConfirm)
            return StepOutcome.Done;
        var data = await StepData.LoadAsync(ctx, p.SessionId, p.ItemIds, p.GroupKeys, _ => true);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        ProposeSummary? summary = null;
        List<ApplyItemReport> applied = [];
        var previous = ctx.Run.ResultJson;
        if (!string.IsNullOrWhiteSpace(previous))
        {
            using var doc = JsonDocument.Parse(previous);
            if (doc.RootElement.TryGetProperty("propose", out var s))
                summary = s.Deserialize<ProposeSummary>(Json.Options);
            if (doc.RootElement.TryGetProperty("apply", out var a))
                applied = a.Deserialize<List<ApplyItemReport>>(Json.Options) ?? [];
        }
        var markdown = BatchReportBuilder.Build(data.Session, data.Items, summary, applied, ctx.Clock.UtcNow);
        await ctx.MergeResultAsync(new { reportMarkdown = markdown });
        return StepOutcome.Info("Apply.BatchReport.Done");
    }
}
