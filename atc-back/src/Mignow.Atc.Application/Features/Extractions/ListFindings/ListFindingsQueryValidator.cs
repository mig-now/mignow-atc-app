using FluentValidation;

namespace Mignow.Atc.Application.Features.Extractions.ListFindings;

public sealed class ListFindingsQueryValidator : AbstractValidator<ListFindingsQuery>
{
    public ListFindingsQueryValidator()
    {
        RuleFor(q => q.Priority).InclusiveBetween(1, 4).When(q => q.Priority is not null);
        RuleFor(q => q.Object).MaximumLength(120);
    }
}
