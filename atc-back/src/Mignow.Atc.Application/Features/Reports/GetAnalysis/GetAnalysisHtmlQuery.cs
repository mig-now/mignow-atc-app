using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Features.Reports.GetAnalysis;

/// <summary>Página HTML do relatório de análise.</summary>
public sealed record GetAnalysisHtmlQuery(Guid CustomerId, string Kind, Guid? SapSystemId = null) : IQuery<Result<string>>;
