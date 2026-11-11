using FluentValidation;

namespace Mignow.Atc.Application.Features.Knowledge.Lookup;

public sealed class KbLookupQueryValidator : AbstractValidator<KbLookupQuery>
{
    public KbLookupQueryValidator()
    {
        RuleFor(q => q.Ref).NotEmpty().MaximumLength(200).WithMessage("O objeto referenciado é obrigatório.");
        RuleFor(q => q.Robt).MaximumLength(10);
        RuleFor(q => q.Top).InclusiveBetween(1, 50).WithMessage("O top deve estar entre 1 e 50.");
    }
}
