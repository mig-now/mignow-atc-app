using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;

namespace Mignow.Atc.Application.Features.Reports.GetAnalysis;

public sealed class GetAnalysisHandler(IDataStore store, IStringLocalizer<Errors> l, IDateTimeProvider clock, IReportTemplate template, IAiGateway ai, IMignowRulesSource rules)
    : IQueryHandler<GetAnalysisQuery, Result<AnalysisResponse>>
{
    public async Task<Result<AnalysisResponse>> Handle(GetAnalysisQuery query, CancellationToken ct)
    {
        var built = await new AnalysisBuilder(store, l, clock, template, ai, rules).BuildAsync(query.CustomerId, query.Kind, query.SapSystemId, ct);
        return built.IsFailure ? built.Error : built.Value.Summary;
    }
}
