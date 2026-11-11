using FluentValidation;

namespace Mignow.Atc.Application.Features.SapSystems.Create;

public sealed class CreateSapSystemCommandValidator : AbstractValidator<CreateSapSystemCommand>
{
    public CreateSapSystemCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        SapSystemRules.Apply(this);
        RuleFor(c => c.Password).NotEmpty().MaximumLength(200).When(c => !SapSystemRules.IsSimulator(c.Host));
    }
}
