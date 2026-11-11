using FluentValidation;
using Mignow.Atc.Application.Modules.Knowledge;

namespace Mignow.Atc.Application.Features.Knowledge.Index;

public sealed class GetKbIndexQueryValidator : AbstractValidator<GetKbIndexQuery>
{
    public GetKbIndexQueryValidator()
    {
        RuleFor(q => q.Category).Must(c => KbCategories.IsValid(c!) || c == "fatos").When(q => !string.IsNullOrWhiteSpace(q.Category))
            .WithMessage("Categoria inválida.");
        RuleFor(q => q.Search).MaximumLength(200);
    }
}
