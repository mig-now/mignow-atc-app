using FluentValidation;
using Mignow.Atc.Application.Modules.Extraction;

namespace Mignow.Atc.Application.Features.Extractions.GetFile;

public sealed class GetExtractionFileQueryValidator : AbstractValidator<GetExtractionFileQuery>
{
    public GetExtractionFileQueryValidator() =>
        RuleFor(q => q.Priorities).Must(p => ExtractionFiles.ParsePriorities(p) is not null).WithErrorCode("Extraction.InvalidPriorities")
            .WithMessage("Extraction.InvalidPriorities");
}
