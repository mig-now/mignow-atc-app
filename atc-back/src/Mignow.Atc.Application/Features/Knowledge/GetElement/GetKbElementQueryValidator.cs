using FluentValidation;

namespace Mignow.Atc.Application.Features.Knowledge.GetElement;

public sealed class GetKbElementQueryValidator : AbstractValidator<GetKbElementQuery>
{
    public GetKbElementQueryValidator()
    {
        RuleFor(q => q.Category).NotEmpty().WithMessage("A categoria é obrigatória.");
        RuleFor(q => q.Element).NotEmpty().MaximumLength(200).WithMessage("O elemento é obrigatório.");
    }
}
