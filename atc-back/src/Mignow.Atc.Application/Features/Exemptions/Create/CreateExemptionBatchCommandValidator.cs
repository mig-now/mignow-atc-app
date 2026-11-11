using FluentValidation;

namespace Mignow.Atc.Application.Features.Exemptions.Create;

public sealed class CreateExemptionBatchCommandValidator : AbstractValidator<CreateExemptionBatchCommand>
{
    public CreateExemptionBatchCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.Priorities).NotEmpty().Must(p => ExemptionRules.ParsePriorities(p) is not null).WithMessage("Prioridades válidas: 1, 2 e/ou 3.");
    }
}
