using FluentValidation;

namespace Mignow.Atc.Application.Features.SessionApply.ActivationDecision;

public sealed class ActivationDecisionCommandValidator : AbstractValidator<ActivationDecisionCommand>
{
    public ActivationDecisionCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.ItemId).NotEmpty();
        RuleFor(c => c.Decision).Must(d => d is "keepInactive" or "rollback").WithMessage("Decision deve ser 'keepInactive' ou 'rollback'.");
    }
}
