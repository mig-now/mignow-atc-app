using FluentValidation;

namespace Mignow.Atc.Application.Features.AiProviders.Create;

public sealed class CreateAiProviderCommandValidator : AbstractValidator<CreateAiProviderCommand>
{
    public CreateAiProviderCommandValidator() => AiProviderRules.Apply(this);
}
