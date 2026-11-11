using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions.ListRequests;

/// <summary>Pedidos do lote por <c>seq</c>; <c>Status</c> filtra (<c>PENDENTE</c> = ainda não simulado).</summary>
public sealed record ListExemptionRequestsQuery(Guid BatchId, int Page = 1, int PageSize = 50, string? Status = null)
    : IQuery<Result<PagedResult<ExemptionRequestResponse>>>;
