using FluentValidation;
using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Features.Scans.StartSimulate;

public sealed class StartSimulateCommandValidator : AbstractValidator<StartSimulateCommand>
{
    public StartSimulateCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Parallel).InclusiveBetween(1, 9).When(c => c.Parallel is not null).WithErrorCode("Scan.InvalidParallel").WithMessage("Scan.InvalidParallel");
        RuleFor(c => c.Lots).Must(l => Regex.IsMatch(l!, @"^\s*\d+(\s*-\s*\d+)?(\s*,\s*\d+(\s*-\s*\d+)?)*\s*$")).When(c => !string.IsNullOrWhiteSpace(c.Lots))
            .WithErrorCode("Scan.InvalidLots").WithMessage("Scan.InvalidLots");
    }
}
