using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.List;

public sealed record ListScansQuery(Guid? SapSystemId, int Page, int PageSize) : IQuery<Result<PagedResult<ScanResponse>>>;
