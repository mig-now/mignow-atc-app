using FluentValidation;

namespace Mignow.Atc.Application.Features.SapSystems.Update;

public sealed class UpdateSapSystemCommandValidator : AbstractValidator<UpdateSapSystemCommand>
{
    public UpdateSapSystemCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        SapSystemRules.Apply(this);
    }
}
