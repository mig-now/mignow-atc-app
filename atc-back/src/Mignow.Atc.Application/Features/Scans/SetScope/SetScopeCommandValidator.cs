using FluentValidation;

namespace Mignow.Atc.Application.Features.Scans.SetScope;

public sealed class SetScopeCommandValidator : AbstractValidator<SetScopeCommand>
{
    public SetScopeCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.RuleIds).NotNull().Must(r => r is { Count: > 0 }).WithErrorCode("Scan.NoRulesSelected").WithMessage("Scan.NoRulesSelected");
    }
}
