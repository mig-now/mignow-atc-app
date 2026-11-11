using FluentValidation;

namespace Mignow.Atc.Application.Features.Sessions.Triage;

public sealed class TriageApplyCommandValidator : AbstractValidator<TriageApplyCommand>
{
    public TriageApplyCommandValidator() => RuleFor(c => c.Rules).NotNull();
}
