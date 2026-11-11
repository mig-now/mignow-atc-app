using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Reports.GetHtml;

/// <summary>Relatório final em HTML (identidade MIGNOW); <c>View</c> = <c>findings</c> (padrão) ou <c>objetos</c>.</summary>
public sealed record GetReportHtmlQuery(Guid CustomerId, string? View = null, Guid? SapSystemId = null) : IQuery<Result<string>>;
