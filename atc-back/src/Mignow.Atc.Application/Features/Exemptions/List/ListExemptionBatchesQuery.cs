using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions.List;

public sealed record ListExemptionBatchesQuery(int Page = 1, int PageSize = 20, Guid? SapSystemId = null, Guid? SessionId = null)
    : IQuery<Result<PagedResult<ExemptionBatchResponse>>>;
