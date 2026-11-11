using FluentValidation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.TriageRules.Save;

public sealed class SaveTriageRuleCommandValidator : AbstractValidator<SaveTriageRuleCommand>
{
    public SaveTriageRuleCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().When(c => c.IsNew);
        RuleFor(c => c.Code).MaximumLength(40).Matches("^[A-Za-z][A-Za-z0-9_]*$")
            .WithMessage("Código: letras, números e _ (começando por letra).").When(c => !string.IsNullOrWhiteSpace(c.Code));
        RuleFor(c => c.Description).NotEmpty().MaximumLength(500);
        RuleFor(c => c.Scope).IsInEnum();
        RuleFor(c => c.Effect).Must(e => TriageRule.Effects.Contains(e)).When(c => c.IsNew)
            .WithMessage($"Efeito: {string.Join(", ", TriageRule.Effects)}.");
        RuleFor(c => c.ObjectPattern).MaximumLength(120);
        RuleFor(c => c.MessagePattern).MaximumLength(200);
        RuleFor(c => c.Obs).MaximumLength(500);
        RuleForEach(c => c.RefObjects).MaximumLength(40);
    }
}
