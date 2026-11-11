using FluentValidation;

namespace Mignow.Atc.Application.Features.Sessions.StartReadSources;

public sealed class StartReadSourcesCommandValidator : AbstractValidator<StartReadSourcesCommand>
{
    public StartReadSourcesCommandValidator() => RuleFor(c => c.Batch).InclusiveBetween(1, 5000).When(c => c.Batch is not null);
}
