using FluentValidation;

namespace Mignow.Atc.Application.Features.SessionApply.Decide;

public sealed class DecideProposalCommandValidator : AbstractValidator<DecideProposalCommand>
{
    public DecideProposalCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.ItemIds).NotEmpty();
        RuleFor(c => c.Decision).Must(d => d is "ConfirmKb" or "ConfirmIa").WithMessage("Decision deve ser 'ConfirmKb' ou 'ConfirmIa'.");
        RuleFor(c => c.Obs).MaximumLength(2000);
        RuleFor(c => c.Reason).MaximumLength(1000);
    }
}
