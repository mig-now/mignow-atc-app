using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.GetSummaryFile;

/// <summary>Resumo do cruzamento em markdown (o <c>resumo.md</c> da skill).</summary>
public sealed record GetSummaryFileQuery(Guid Id) : IQuery<Result<ScanFileResponse>>;
