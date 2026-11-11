using FluentValidation;

namespace Mignow.Atc.Application.Features.Reports.Highlights;

public sealed class UpdateHighlightsCommandValidator : AbstractValidator<UpdateHighlightsCommand>
{
    public UpdateHighlightsCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.Highlights).NotNull().Must(h => h is null || h.Count <= 50).WithMessage("Report.TooManyHighlights");
        RuleFor(c => c.Pending).NotNull().Must(p => p is null || p.Count <= 50).WithMessage("Report.TooManyHighlights");
        RuleForEach(c => c.Highlights).ChildRules(h => h.RuleFor(x => x.Texto).NotEmpty().MaximumLength(2000));
        RuleForEach(c => c.Pending).ChildRules(p =>
        {
            p.RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
            p.RuleFor(x => x.Texto).NotEmpty().MaximumLength(2000);
        });
        When(c => c.Premise is not null, () =>
        {
            RuleFor(c => c.Premise!.ObjectsPerDay).GreaterThanOrEqualTo(1).When(c => c.Premise!.ObjectsPerDay is not null);
            RuleFor(c => c.Premise!.HoursPerDay).InclusiveBetween(1, 24).When(c => c.Premise!.HoursPerDay is not null);
        });
    }
}
