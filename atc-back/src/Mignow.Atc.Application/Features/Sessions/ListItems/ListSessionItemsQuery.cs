using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Sessions.ListItems;

/// <summary>
/// Filtros: <c>Status</c> (pending|confirmedKb|confirmedAi|cancelled|exemption, ou o texto exato da skill; <c>applyError</c> = última tentativa no SAP falhou; <c>approval</c> = pendente de aprovação (proposta esperando o consultor); <c>unprocessed</c> = pendente ainda sem proposta), <c>Priority</c>, <c>ObjectName</c> (contém),
/// <c>Group</c> (objeto referenciado) e <c>Triage</c> (regra que tocou o item: R1…R9, produto).
/// </summary>
public sealed record ListSessionItemsQuery(Guid SessionId, string? Status, string? Priority, string? ObjectName, string? Group, string? Triage, int Page = 1, int PageSize = 50)
    : IQuery<Result<PagedResult<ItemResponse>>>;
