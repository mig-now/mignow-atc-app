using FluentValidation;

namespace Mignow.Atc.Application.Features.Sessions.ImportCsv;

public sealed class ImportCsvSessionCommandValidator : AbstractValidator<ImportCsvSessionCommand>
{
    public ImportCsvSessionCommandValidator()
    {
        RuleFor(c => c.SapSystemId).NotEmpty();
        RuleFor(c => c.Content).NotEmpty();
        RuleFor(c => c.Priorities).MaximumLength(20).Matches(@"^\s*(todas|[0-9]+(\s*,\s*[0-9]+)*)?\s*$").When(c => c.Priorities is not null);
        RuleFor(c => c.Request).MaximumLength(20);
    }
}
