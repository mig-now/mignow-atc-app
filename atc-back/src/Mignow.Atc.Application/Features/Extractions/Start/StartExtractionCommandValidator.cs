using FluentValidation;

namespace Mignow.Atc.Application.Features.Extractions.Start;

public sealed class StartExtractionCommandValidator : AbstractValidator<StartExtractionCommand>
{
    public StartExtractionCommandValidator() => RuleFor(c => c.SapSystemId).NotEmpty();
}
