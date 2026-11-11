using FluentValidation;

namespace Mignow.Atc.Application.Features.Scans.Create;

public sealed class CreateScanCommandValidator : AbstractValidator<CreateScanCommand>
{
    public CreateScanCommandValidator() => RuleFor(c => c.SapSystemId).NotEmpty();
}
