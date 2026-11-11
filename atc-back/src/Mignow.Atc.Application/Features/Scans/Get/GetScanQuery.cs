using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.Get;

public sealed record GetScanQuery(Guid Id) : IQuery<Result<ScanResponse>>;
