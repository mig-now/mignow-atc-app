using FluentValidation;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Features.Scans.ListOccurrences;

public sealed class ListOccurrencesQueryValidator : AbstractValidator<ListOccurrencesQuery>
{
    public ListOccurrencesQueryValidator()
    {
        RuleFor(q => q.Cross).Must(c => ScanCross.All.Contains(c!)).When(q => !string.IsNullOrEmpty(q.Cross)).WithErrorCode("Scan.InvalidCross").WithMessage("Scan.InvalidCross");
        RuleFor(q => q.Status).Must(s => s is "P" or "R" or "I").When(q => !string.IsNullOrEmpty(q.Status)).WithErrorCode("Scan.InvalidStatus").WithMessage("Scan.InvalidStatus");
    }
}
