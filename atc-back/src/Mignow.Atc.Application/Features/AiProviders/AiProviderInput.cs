using FluentValidation;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.AiProviders;

/// <summary>Campos comuns de criação e alteração. <see cref="ApiKey"/> nulo = manter a atual (nunca volta pela API).</summary>
public interface IAiProviderInput
{
    string Name { get; }
    AiProviderKind Kind { get; }
    string Model { get; }
    string? ApiKey { get; }
    bool IsDefault { get; }
}

internal static class AiProviderRules
{
    public static void Apply<T>(AbstractValidator<T> v) where T : IAiProviderInput
    {
        v.RuleFor(c => c.Name).NotEmpty().MaximumLength(120);
        v.RuleFor(c => c.Model).NotEmpty().MaximumLength(120);
        v.RuleFor(c => c.Kind).IsInEnum();
        v.RuleFor(c => c.ApiKey).MaximumLength(500);
    }
}
