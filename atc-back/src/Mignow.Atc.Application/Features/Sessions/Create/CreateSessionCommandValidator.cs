using FluentValidation;

namespace Mignow.Atc.Application.Features.Sessions.Create;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(c => c.ExtractionId).NotEmpty();
        RuleFor(c => c.Priorities).MaximumLength(20).Matches(@"^\s*(todas|[0-9]+(\s*,\s*[0-9]+)*)?\s*$").When(c => c.Priorities is not null);
        RuleFor(c => c.Request).MaximumLength(20);
    }
}
