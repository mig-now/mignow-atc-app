using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation;

namespace Mignow.Atc.Application.Features.Sessions.Export;

/// <summary>CSV original + coluna <c>Status Remediação</c> (<c>atc_status.csv</c>).</summary>
public sealed record ExportSessionQuery(Guid SessionId) : IQuery<Result<ExportedCsv>>;
