using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Reports.GetAnalysis;

/// <summary>Resumo do relatório de análise (<c>isencao</c>, <c>pendencias</c> ou <c>mignow</c>).</summary>
public sealed record GetAnalysisQuery(Guid CustomerId, string Kind, Guid? SapSystemId = null) : IQuery<Result<AnalysisResponse>>;
