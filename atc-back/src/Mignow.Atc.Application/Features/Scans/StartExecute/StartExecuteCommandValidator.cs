using FluentValidation;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Services;
using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Features.Scans.StartExecute;

public sealed class StartExecuteCommandValidator : AbstractValidator<StartExecuteCommand>
{
    public StartExecuteCommandValidator(IOptions<SapConcurrencyOptions> sap)
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Parallel).InclusiveBetween(1, 9).When(c => c.Parallel is not null).WithErrorCode("Scan.InvalidParallel").WithMessage("Scan.InvalidParallel");
        RuleFor(c => c.Parallel).LessThanOrEqualTo(_ => sap.Value.MaxMignowParallel).When(c => c.Parallel is >= 1 and <= 9)
            .WithErrorCode("Settings.MignowParallelAboveAdtLimit").WithMessage("Settings.MignowParallelAboveAdtLimit");
        RuleFor(c => c.Lots).Must(l => Regex.IsMatch(l!, @"^\s*\d+(\s*-\s*\d+)?(\s*,\s*\d+(\s*-\s*\d+)?)*\s*$")).When(c => !string.IsNullOrWhiteSpace(c.Lots))
            .WithErrorCode("Scan.InvalidLots").WithMessage("Scan.InvalidLots");
    }
}
