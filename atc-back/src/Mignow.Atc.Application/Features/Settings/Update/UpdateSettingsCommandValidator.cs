using FluentValidation;
using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Settings.Update;

public sealed class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator(IOptions<SapConcurrencyOptions> sap)
    {
        // Faixas (também em [Range] no comando, para o Swagger): Parallelism e SourceReadParallelism 1–16, MignowParallel 1–9 (e dentro do limite do ADT),
        // BatchSize/MignowWeightPerBatch/ManualObjectsPerDay >= 1, HoursPerDay 1–24.
        RuleFor(c => c.Parallelism).InclusiveBetween(1, GeneralSettings.MaxParallelism).WithErrorCode("Settings.InvalidParallelism").WithMessage("Settings.InvalidParallelism");
        RuleFor(c => c.SourceReadParallelism).InclusiveBetween(1, GeneralSettings.MaxParallelism).WithErrorCode("Settings.InvalidParallelism").WithMessage("Settings.InvalidParallelism");
        RuleFor(c => c.BatchSize).GreaterThanOrEqualTo(1).WithErrorCode("Settings.InvalidNumber").WithMessage("Settings.InvalidNumber");
        RuleFor(c => c.KbPath).NotEmpty().MaximumLength(500).WithErrorCode("Settings.KbPathRequired").WithMessage("Settings.KbPathRequired");
        RuleFor(c => c.MignowRulesPath).NotEmpty().MaximumLength(500).WithErrorCode("Settings.MignowRulesPathRequired").WithMessage("Settings.MignowRulesPathRequired");
        RuleFor(c => c.MignowParallel).InclusiveBetween(1, GeneralSettings.MaxMignowParallel).WithErrorCode("Settings.InvalidMignowParallel").WithMessage("Settings.InvalidMignowParallel");
        RuleFor(c => c.MignowParallel).LessThanOrEqualTo(_ => sap.Value.MaxMignowParallel)
            .When(c => c.MignowParallel is >= 1 and <= GeneralSettings.MaxMignowParallel)
            .WithErrorCode("Settings.MignowParallelAboveAdtLimit").WithMessage("Settings.MignowParallelAboveAdtLimit");
        RuleFor(c => c.MignowWeightPerBatch).GreaterThanOrEqualTo(1).WithErrorCode("Settings.InvalidNumber").WithMessage("Settings.InvalidNumber");
        RuleFor(c => c.ManualObjectsPerDay).GreaterThanOrEqualTo(1).WithErrorCode("Settings.InvalidNumber").WithMessage("Settings.InvalidNumber");
        RuleFor(c => c.HoursPerDay).InclusiveBetween(1, 24).WithErrorCode("Settings.InvalidNumber").WithMessage("Settings.InvalidNumber");
    }
}
