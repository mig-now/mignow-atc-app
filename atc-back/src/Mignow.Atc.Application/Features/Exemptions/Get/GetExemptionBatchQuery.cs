using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions.Get;

/// <summary>Lote com os totais por status dos pedidos e as quantidades do histórico.</summary>
public sealed record GetExemptionBatchQuery(Guid Id) : IQuery<Result<ExemptionBatchResponse>>;
