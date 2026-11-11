using FluentValidation;

namespace Mignow.Atc.Application.Features.SessionApply.Propose;

public sealed class ProposeCommandValidator : AbstractValidator<ProposeCommand>
{
    public ProposeCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.Mode).Must(m => m is "kb" or "ia").WithMessage("Mode deve ser 'kb' ou 'ia'.");
    }
}
