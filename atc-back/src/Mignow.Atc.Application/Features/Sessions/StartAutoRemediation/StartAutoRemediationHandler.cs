using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Runs;
using Mignow.Atc.Application.Engine.RemediationAuto;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.StartAutoRemediation;

/// <summary>
/// Enfileira o play. Recusa antes de começar o que faria a aplicação falhar no fim: sessão arquivada, sem request, ambiente de produção,
/// item de outra sessão ou nenhum item pendente nem com erro de gravação para reaplicar. A categoria do mandante (T000) é conferida na gravação, como em toda escrita.
/// </summary>
public sealed class StartAutoRemediationHandler(IDataStore store, RunStarter starter, IProductRules products, IStringLocalizer<Errors> l)
    : ICommandHandler<StartAutoRemediationCommand, Result<RunResponse>>
{
    public async Task<Result<RunResponse>> Handle(StartAutoRemediationCommand command, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, command.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        if (session.Archived)
            return l.Conflict("Session.Archived");
        if (string.IsNullOrWhiteSpace(session.Request))
            return l.Conflict("Apply.RequestRequired");
        var system = await SessionViews.LoadSystemAsync(store, session.SapSystemId, ct);
        if (system is null)
            return l.NotFound("SapSystem.NotFound");
        if (system.IsProduction)
            return l.Forbidden("Sap.WriteBlockedProduction", system.Client);
        // regras de triagem do cadastro que valem para o tipo de projeto (genéricas + específicas dele); o consultor não desmarca
        var triageRules = TriageEngine.RulesFor(session.ProjectType, await TriageRuleSource.LoadAsync(store, products, ct)).Select(r => r.Code).ToList();
        var sessionId = session.Id;
        var ids = command.ItemIds?.Distinct().ToArray() ?? [];
        var items = await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == sessionId && (ids.Length == 0 || ids.Contains(i.Guid)))
            .Select(i => new { i.Guid, i.Status, i.ApplicationResult }), ct);
        if (ids.Length > 0 && items.Count != ids.Length)
            return l.NotFound("Item.NotFound");
        // confirmados cuja gravação falhou (request, lock…) entram para reaplicar; erro de ativação/rollback ficam com o consultor
        var reprocess = items.Where(i => i.Status is RemediationStatus.ConfirmedKb or RemediationStatus.ConfirmedAi && ApplyResults.Reprocessable(i.ApplicationResult))
            .Select(i => i.Guid).ToList();
        if (!items.Any(i => i.Status == RemediationStatus.Pending) && reprocess.Count == 0)
            return l.Conflict("AutoRemediation.NothingPending");
        var run = await starter.EnqueueAsync(RunKind.AutoRemediation, system, session.Guid,
            AutoRemediationParameters.For(session.Guid, ids, triageRules, reprocess), ct);
        return run.IsFailure ? run.Error : await RunViews.BuildAsync(store, run.Value, ct);
    }
}
