using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Extractions;

namespace Mignow.Atc.Application.Features.Extractions.Monitor;

/// <summary>Monitor de extrações: ambientes ativos (de um cliente, opcional) com a última execução e a última extração.</summary>
public sealed record GetExtractionMonitorQuery(Guid? CustomerId = null) : IQuery<Result<IReadOnlyList<ExtractionMonitorRow>>>;
