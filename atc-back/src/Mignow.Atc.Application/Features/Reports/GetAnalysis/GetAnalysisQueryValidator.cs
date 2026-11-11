using FluentValidation;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Application.Features.Reports.GetAnalysis;

public sealed class GetAnalysisQueryValidator : AbstractValidator<GetAnalysisQuery>
{
    public GetAnalysisQueryValidator() =>
        RuleFor(q => q.Kind).Must(k => ReportAnalysisKinds.Parse(k) is not null).WithErrorCode("Report.InvalidAnalysis").WithMessage("Report.InvalidAnalysis");
}
