using FluentValidation;

namespace Mignow.Atc.Application.Features.Sessions.StartAutoRemediation;

public sealed class StartAutoRemediationCommandValidator : AbstractValidator<StartAutoRemediationCommand>
{
    public StartAutoRemediationCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleForEach(c => c.ItemIds).NotEmpty();
    }
}
