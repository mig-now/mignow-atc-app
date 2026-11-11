using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>1º passo de <c>ConfirmR1</c>: consulta o DDIC (<see cref="R1Checker"/>) dos itens da R1 e grava as propostas no resultado do run. Não altera itens.</summary>
public sealed class ConfirmR1CheckStep(ISapSql sql) : IRunStep
{
    public string Code => RemediationStepCodes.ConfirmR1Check;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ConfirmR1Parameters>();
        var items = await ctx.WithDbAsync(async () =>
        {
            var session = await ctx.Store.FirstOrDefaultAsync(ctx.Store.Read<RemediationSession>().Where(s => s.Guid == p.SessionId), ctx.Ct);
            if (session is null)
                return null;
            var id = session.Id;
            return await ctx.Store.ListAsync(ctx.Store.Read<RemediationItem>().Where(i => i.SessionId == id).OrderBy(i => i.N), ctx.Ct);
        });
        if (items is null)
            return StepOutcome.Stop("Session.NotFound");
        var targets = R1Checker.Targets(items, p.ItemNs);
        await ctx.ReportProgressAsync(0, targets.Count);
        var checks = await new R1Checker(sql).CheckAsync(ctx.SapConnection, targets, ctx.Ct);
        await ctx.MergeResultAsync(new
        {
            items = checks.Count,
            exemption = checks.Where(c => c.Proposal == R1Check.Exemption).Select(c => c.N),
            pending = checks.Where(c => c.Proposal == R1Check.Pending).Select(c => c.N),
            error = checks.Where(c => c.Proposal == R1Check.Error).Select(c => c.N),
            checks,
        });
        await ctx.ReportProgressAsync(targets.Count, targets.Count);
        return StepOutcome.Info("Remediation.ConfirmR1Checked", Json.Args(("items", checks.Count)));
    }
}
