using FluentValidation;

namespace Mignow.Atc.Application.Features.Exemptions.UpdateParams;

public sealed class UpdateExemptionParamsCommandValidator : AbstractValidator<UpdateExemptionParamsCommand>
{
    public UpdateExemptionParamsCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Approver).MaximumLength(40);
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(10);
        RuleFor(c => c.Validity).NotEmpty().Must(v => ExemptionRules.ValidValidities.Contains(v?.Trim().ToUpperInvariant() ?? string.Empty))
            .WithMessage("Validade: U, D ou P.");
        RuleFor(c => c.JustificationPrefix).MaximumLength(200);
    }
}
