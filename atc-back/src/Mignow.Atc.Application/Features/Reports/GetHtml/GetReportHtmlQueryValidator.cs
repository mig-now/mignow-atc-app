using FluentValidation;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Application.Features.Reports.GetHtml;

public sealed class GetReportHtmlQueryValidator : AbstractValidator<GetReportHtmlQuery>
{
    public GetReportHtmlQueryValidator() =>
        RuleFor(q => q.View).Must(v => v is null || ReportHtmlRenderer.IsView(v)).WithErrorCode("Report.InvalidView").WithMessage("Report.InvalidView");
}
