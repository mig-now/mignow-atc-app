using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Features.Sessions;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>
/// Triagem automática dos itens escolhidos (porte do <c>triagem --aplicar</c> com as regras do tipo de projeto da sessão): classifica com os fontes lidos, registra as
/// regras em <c>TriageFlags</c> e grava o status/obs exatos das regras que gravam (R1, R2, R5, R7, R8, R9 e produtos MIGNOW). As que só
/// sinalizam (R3, R4, R6) não mudam o item. O baseline já ficou fora na criação da sessão (<c>_novos</c>). Idempotente.
/// </summary>
public sealed class AutoTriageStep(IProductRules products) : IRunStep
{
    public string Code => AutoRemediationStepCodes.Triage;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var p = ctx.Parameters<AutoRemediationParameters>();
        var data = await AutoRemediationItems.LoadAsync(ctx, p);
        if (data is null)
            return StepOutcome.Stop("Session.NotFound");
        var (session, all, selected) = data.Value;
        var selectedNs = selected.Select(i => i.N).ToHashSet();
        var pendingNames = all.Where(i => i.Status == RemediationStatus.Pending).Select(i => i.ObjectName).ToList();
        var sources = await ctx.WithDbAsync(() => SessionViews.SourcesAsync(ctx.Store, session.SapSystemId, pendingNames, ctx.Ct));
        var rules = await ctx.WithDbAsync(() => TriageRuleSource.LoadAsync(ctx.Store, products, ctx.Ct));
        var result = TriageEngine.Classify(all, sources, session.ProjectType, products.Get(), rules);
        var now = ctx.Clock.UtcNow;
        var applied = new Dictionary<string, int>(StringComparer.Ordinal);
        // só as regras do tipo de projeto gravam status (null = todas, runs antigos); a sinalização em TriageFlags vale para todas, como na triagem manual
        var chosen = p.TriageRules is null ? null : p.TriageRules.ToHashSet(StringComparer.OrdinalIgnoreCase);
        await ctx.MutateAsync(() =>
        {
            foreach (var rule in result.Rules)
                foreach (var hit in rule.Items.Where(h => selectedNs.Contains(h.N)))
                {
                    var item = all.First(i => i.N == hit.N);
                    item.SetTriageFlags([.. item.TriageFlags, rule.Code]);
                    if ((chosen is not null && !chosen.Contains(rule.Code)) || TriageEngine.StatusOf(rule) is not { } status
                        || item.Status != RemediationStatus.Pending)
                        continue;
                    item.MarkStatus(status, hit.Obs, now);
                    applied[rule.Code] = applied.GetValueOrDefault(rule.Code) + 1;
                }
        });
        await ctx.SaveAsync();
        await ctx.MergeResultAsync(new { triage = applied, triageRules = p.TriageRules });
        await ctx.ReportProgressAsync(1, 1);
        return StepOutcome.Info("AutoRemediation.TriageApplied", Json.Args(("items", applied.Values.Sum()), ("rules", string.Join(", ", applied.Keys))));
    }
}
