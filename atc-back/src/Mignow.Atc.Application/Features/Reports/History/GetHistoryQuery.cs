using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Application.Features.Reports.History;

/// <summary>Histórico de processo de um módulo (<c>extraction</c>, <c>remediation</c>, <c>exemption</c>, <c>scan</c>) por cliente e/ou ambiente (§3.8 do desenho).</summary>
public sealed record GetHistoryQuery(Guid? CustomerId, Guid? SapSystemId, string Module) : IQuery<Result<IReadOnlyList<ProcessHistoryEntry>>>;
