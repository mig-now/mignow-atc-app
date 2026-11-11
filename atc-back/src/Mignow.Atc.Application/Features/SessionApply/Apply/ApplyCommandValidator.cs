using FluentValidation;

namespace Mignow.Atc.Application.Features.SessionApply.Apply;

public sealed class ApplyCommandValidator : AbstractValidator<ApplyCommand>
{
    public ApplyCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.ItemIds).NotEmpty();
    }
}
