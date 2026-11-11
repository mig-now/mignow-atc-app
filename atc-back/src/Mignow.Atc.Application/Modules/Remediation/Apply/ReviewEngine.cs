using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Modules.Remediation.Apply.Ai;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Revisão por IA antes de gravar (uso Review, prompt derivado do agente <c>sap-atc-abap</c>): lê a proposta já simulada e grava o parecer em
/// <c>Proposal.Review</c>. Em paralelo (<c>GeneralSettings.Parallelism</c>); só itens simulados com proposta de correção.
/// </summary>
public sealed class ReviewEngine(IAiGateway ai, IStringLocalizer<Errors> l)
{
    public async Task<int> ReviewAsync(IReadOnlyList<RemediationItem> items, EngineHost host)
    {
        var todo = items.Where(i => i.Proposal is { Action: ItemProposal.ActionFix } && i.Application?.Result == ApplyResults.Simulated).ToList();
        var reviewed = 0;
        await Parallel.ForEachAsync(todo, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, host.Settings.Parallelism), CancellationToken = host.Ct }, async (item, _) =>
        {
            if (await host.IsCancelledAsync())
                return;
            var p = item.Proposal!;
            var task = new ReviewTask(item.N, item.ObjectName, item.ObjectType, item.Check, item.Message, item.Note, p.Origin, p.KbCardId, p.Justification, p.Alerts, p.SyntaxOk,
                p.SyntaxMessages ?? [], item.Preparation?.Diff ?? p.Diff ?? string.Empty, [.. (item.Preparation?.Related ?? []).Select(r => r.ObjectName)]);
            var request = new AiRequest(AiUsage.Review, RemediationPurposes.Review, ReviewPrompts.System, ReviewPrompts.User(task), ReviewPrompts.Schema, AiEffort.Medium);
            var result = await ai.CompleteAsync<ReviewAiResponse>(request, u => host.AddAiUsage(u, AiUsage.Review, $"item {item.N}"), host.Ct);
            if (result.IsFailure)
            {
                await host.LogAsync(EventLevel.Warning, result.Error.Code, null, $"item {item.N}");
                return;
            }
            var text = ReviewPrompts.Render(result.Value);
            await host.MutateAsync(() => item.SetProposal(item.Proposal! with { Review = text }));
            Interlocked.Increment(ref reviewed);
        });
        _ = l;
        await host.SaveAsync();
        return reviewed;
    }
}
