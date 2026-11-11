using FluentValidation;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Application.Features.Reports.History;

public sealed class GetHistoryQueryValidator : AbstractValidator<GetHistoryQuery>
{
    public GetHistoryQueryValidator()
    {
        RuleFor(q => q.Module).Must(m => Enum.TryParse<ProcessModule>(m, true, out _)).WithErrorCode("Report.InvalidModule").WithMessage("Report.InvalidModule");
        RuleFor(q => q).Must(q => q.CustomerId is not null || q.SapSystemId is not null).WithErrorCode("Report.HistoryFilterRequired").WithMessage("Report.HistoryFilterRequired");
    }
}
