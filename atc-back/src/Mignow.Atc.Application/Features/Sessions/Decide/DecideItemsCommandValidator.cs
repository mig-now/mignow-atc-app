using FluentValidation;

namespace Mignow.Atc.Application.Features.Sessions.Decide;

public sealed class DecideItemsCommandValidator : AbstractValidator<DecideItemsCommand>
{
    public DecideItemsCommandValidator()
    {
        RuleFor(c => c.ItemIds).NotEmpty();
        RuleFor(c => c.Decision).IsInEnum();
        RuleFor(c => c.Obs).MaximumLength(4000);
        RuleFor(c => c.Reason).MaximumLength(2000);
    }
}
