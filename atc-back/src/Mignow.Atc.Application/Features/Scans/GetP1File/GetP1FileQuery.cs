using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Scans;

namespace Mignow.Atc.Application.Features.Scans.GetP1File;

/// <summary>CSV P1 (<c>mignow_&lt;SID&gt;_&lt;MANDT&gt;_&lt;data&gt;_remediacao.csv</c>). <see cref="IncludeOtherLine"/> nulo = o que o cruzamento usou.</summary>
public sealed record GetP1FileQuery(Guid Id, bool? IncludeOtherLine) : IQuery<Result<ScanFileResponse>>;
