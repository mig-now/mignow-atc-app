using FluentValidation;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Application.Features.Reports.GetAnalysis;

public sealed class GetAnalysisHtmlQueryValidator : AbstractValidator<GetAnalysisHtmlQuery>
{
    public GetAnalysisHtmlQueryValidator() =>
        RuleFor(q => q.Kind).Must(k => ReportAnalysisKinds.Parse(k) is not null).WithErrorCode("Report.InvalidAnalysis").WithMessage("Report.InvalidAnalysis");
}
