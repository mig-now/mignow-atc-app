using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Application.Features.Reports.GetData;

/// <summary>Dados do relatório final (= <c>dados.json</c> do <c>coletar.py</c>); sem ambiente, o do cliente com a sessão mais recente.</summary>
public sealed record GetReportQuery(Guid CustomerId, Guid? SapSystemId = null) : IQuery<Result<ReportData>>;
