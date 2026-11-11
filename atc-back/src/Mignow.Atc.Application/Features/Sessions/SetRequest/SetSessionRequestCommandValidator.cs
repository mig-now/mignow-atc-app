using FluentValidation;

namespace Mignow.Atc.Application.Features.Sessions.SetRequest;

public sealed class SetSessionRequestCommandValidator : AbstractValidator<SetSessionRequestCommand>
{
    public SetSessionRequestCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.Request).NotEmpty().MaximumLength(20);
    }
}
