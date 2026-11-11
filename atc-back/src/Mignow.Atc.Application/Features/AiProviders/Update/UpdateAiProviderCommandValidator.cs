using FluentValidation;

namespace Mignow.Atc.Application.Features.AiProviders.Update;

public sealed class UpdateAiProviderCommandValidator : AbstractValidator<UpdateAiProviderCommand>
{
    public UpdateAiProviderCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        AiProviderRules.Apply(this);
    }
}
