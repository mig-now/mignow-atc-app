using FluentValidation;

namespace Mignow.Atc.Application.Features.SessionApply.ApplySimulate;

public sealed class ApplySimulateCommandValidator : AbstractValidator<ApplySimulateCommand>
{
    public ApplySimulateCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.ItemIds).NotEmpty();
    }
}
