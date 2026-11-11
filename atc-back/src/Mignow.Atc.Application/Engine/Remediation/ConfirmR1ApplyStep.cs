using System.Text.Json;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>
/// 2º passo de <c>ConfirmR1</c> (<c>conferir-r1 --aplicar</c>): grava Isenção (só chave/verificação) ou volta o item a pendente com
/// "Conferência R1: …". Sem <c>Apply</c> só propõe. Idempotente (mesmo resultado ao repetir).
/// </summary>
public sealed class ConfirmR1ApplyStep : IRunStep
{
    public string Code => RemediationStepCodes.ConfirmR1Apply;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<ConfirmR1Parameters>();
        if (!p.Apply || string.IsNullOrWhiteSpace(ctx.Run.ResultJson))
            return StepOutcome.Info("Remediation.ConfirmR1NotApplied");
        using var doc = JsonDocument.Parse(ctx.Run.ResultJson);
        var checks = doc.RootElement.TryGetProperty("checks", out var c) ? c.Deserialize<List<R1Check>>(Json.Options) ?? [] : [];
        var applied = new List<int>();
        var now = ctx.Clock.UtcNow;
        await ctx.WithDbAsync(async () =>
        {
            var session = await ctx.Store.FirstOrDefaultAsync(ctx.Store.Read<RemediationSession>().Where(s => s.Guid == p.SessionId), ctx.Ct);
            if (session is null)
                return;
            var id = session.Id;
            var ns = checks.Select(x => x.N).ToArray();
            var items = await ctx.Store.ListAsync(ctx.Store.Query<RemediationItem>().Where(i => i.SessionId == id && ns.Contains(i.N)), ctx.Ct);
            foreach (var check in checks.Where(x => x.Proposal is R1Check.Exemption or R1Check.Pending))
            {
                if (items.FirstOrDefault(i => i.N == check.N) is not { } item)
                    continue;
                R1Checker.Apply(item, check, now);
                applied.Add(check.N);
            }
        });
        await ctx.SaveAsync();
        await ctx.LogAsync(Domain.Execution.EventLevel.Info, "Remediation.ConfirmR1Applied", Json.Args(("items", applied.Count)));
        return StepOutcome.Info("Remediation.ConfirmR1Applied", Json.Args(("items", applied.Count)));
    }
}
