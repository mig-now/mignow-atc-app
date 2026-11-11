using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Sessions.Triage;

/// <summary>
/// Reclassifica agora (não confia na prévia) e grava o status e a obs exatos do script em cada item das regras pedidas; registra em
/// <c>TriageFlags</c> a regra que tocou cada item classificado (inclusive as que só sinalizam).
/// </summary>
public sealed class TriageApplyHandler(IDataStore store, IProductRules products, IDateTimeProvider clock, IStringLocalizer<Errors> l)
    : ICommandHandler<TriageApplyCommand, Result<TriageResponse>>
{
    public async Task<Result<TriageResponse>> Handle(TriageApplyCommand command, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, command.SessionId, true, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        if (session.Archived)
            return l.Conflict("Session.Archived");
        var (items, result) = await TriageSupport.ClassifyAsync(store, products, session, true, ct);
        var requested = command.Rules.Select(r => r.Trim().ToUpperInvariant()).Where(r => r.Length > 0).Distinct().ToList();
        var unknown = requested.FirstOrDefault(r => result.Rule(r) is null);
        if (unknown is not null)
            return l.Validation("Triage.UnknownRule", unknown, string.Join(", ", result.Rules.Select(r => r.Code)));
        var now = clock.UtcNow;
        foreach (var rule in result.Rules)
            foreach (var hit in rule.Items)
            {
                var item = items.First(i => i.N == hit.N);
                item.SetTriageFlags([.. item.TriageFlags, rule.Code]);
            }
        var applied = new List<int>();
        foreach (var code in requested)
        {
            var status = TriageEngine.StatusOf(result.Rule(code)!);
            if (status is null)
                continue;
            foreach (var hit in result.Rule(code)!.Items)
            {
                items.First(i => i.N == hit.N).MarkStatus(status, hit.Obs, now);
                applied.Add(hit.N);
            }
        }
        return TriageSupport.ToResponse(result, items.Count(i => i.Status.Length == 0), applied);
    }
}
