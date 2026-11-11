using FluentValidation;

namespace Mignow.Atc.Application.Features.SapSystems.ChangePassword;

public sealed class ChangeSapSystemPasswordCommandValidator : AbstractValidator<ChangeSapSystemPasswordCommand>
{
    public ChangeSapSystemPasswordCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Password).NotEmpty().MaximumLength(200);
    }
}
