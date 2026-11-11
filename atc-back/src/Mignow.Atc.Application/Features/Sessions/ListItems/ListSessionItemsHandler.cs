using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation.Apply;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.ListItems;

public sealed class ListSessionItemsHandler(IDataStore store, IStringLocalizer<Errors> l) : IQueryHandler<ListSessionItemsQuery, Result<PagedResult<ItemResponse>>>
{
    public const string ApplyErrorFilter = "applyError";
    public const string ApprovalFilter = "approval";
    public const string UnprocessedFilter = "unprocessed";

    public static string? ParseStatus(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        null or "" => null,
        "pending" or "pendente" => RemediationStatus.Pending,
        "confirmedkb" or "confirmado kb" => RemediationStatus.ConfirmedKb,
        "confirmedai" or "confirmado ia" => RemediationStatus.ConfirmedAi,
        "cancelled" or "cancelado" => RemediationStatus.Cancelled,
        "exemption" or "isenção" or "isencao" => RemediationStatus.Exemption,
        _ => "?",
    };

    public async Task<Result<PagedResult<ItemResponse>>> Handle(ListSessionItemsQuery query, CancellationToken ct)
    {
        var session = await SessionViews.LoadAsync(store, query.SessionId, false, ct);
        if (session is null)
            return l.NotFound("Session.NotFound");
        var sessionId = session.Id;
        var q = store.Read<RemediationItem>().Where(i => i.SessionId == sessionId);
        // "applyError": a última tentativa no SAP falhou, qualquer que seja a situação (o "Confirmado" é só a decisão)
        var applyError = string.Equals(query.Status?.Trim(), ApplyErrorFilter, StringComparison.OrdinalIgnoreCase);
        // "approval": pendente com proposta esperando a decisão do consultor; "unprocessed": pendente ainda sem proposta (ou só sem cartão na KB)
        var filter = query.Status?.Trim();
        var approval = string.Equals(filter, ApprovalFilter, StringComparison.OrdinalIgnoreCase);
        var unprocessed = string.Equals(filter, UnprocessedFilter, StringComparison.OrdinalIgnoreCase);
        var status = applyError || approval || unprocessed ? null : ParseStatus(query.Status);
        if (applyError)
            q = q.Where(i => i.ApplicationResult != null && !ApplyResults.NotFailed.Contains(i.ApplicationResult));
        if (approval || unprocessed)
            q = q.Where(i => i.Status == RemediationStatus.Pending && i.AwaitingApproval == approval);
        if (status == "?")
            return l.Validation("Session.InvalidStatusFilter");
        if (status is not null)
            q = q.Where(i => i.Status == status);
        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            var p = query.Priority.Trim();
            q = q.Where(i => i.Priority == p);
        }
        if (!string.IsNullOrWhiteSpace(query.ObjectName))
        {
            var o = query.ObjectName.Trim().ToUpper();
            q = q.Where(i => i.ObjectName.ToUpper().Contains(o));
        }
        if (!string.IsNullOrWhiteSpace(query.Group))
        {
            var g = query.Group.Trim().ToUpper();
            q = q.Where(i => i.RefObject != null && i.RefObject.ToUpper() == g);
        }
        q = q.OrderBy(i => i.N);
        if (string.IsNullOrWhiteSpace(query.Triage))
        {
            var paged = await store.PageAsync(q, query.Page, query.PageSize, ct);
            return new PagedResult<ItemResponse>([.. paged.Items.Select(i => i.ToResponse())], paged.Page, paged.PageSize, paged.Total);
        }
        // TriageFlags é coluna de conversão JSON: o filtro roda em memória sobre o que passou nos demais filtros.
        var flag = query.Triage.Trim();
        var all = (await store.ListAsync(q, ct)).Where(i => i.TriageFlags.Contains(flag, StringComparer.OrdinalIgnoreCase)).ToList();
        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.PageSize, 1, 200);
        return new PagedResult<ItemResponse>([.. all.Skip((page - 1) * size).Take(size).Select(i => i.ToResponse())], page, size, all.Count);
    }
}
