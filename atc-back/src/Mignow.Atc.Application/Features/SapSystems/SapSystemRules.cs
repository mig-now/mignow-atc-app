using FluentValidation;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.SapSystems;

/// <summary>Campos comuns de criação e alteração do ambiente.</summary>
public interface ISapSystemInput
{
    string Sid { get; }
    string Client { get; }
    string Host { get; }
    int AdtPort { get; }
    bool Https { get; }
    string Language { get; }
    string? CertThumbprint { get; }
    ProjectType ProjectType { get; }
    string Author { get; }
    string? DefaultRequest { get; }
    string? ExemptionApprover { get; }
    string Username { get; }
}

internal static class SapSystemRules
{
    public static bool IsSimulator(string? host) => host?.Trim().StartsWith(SapSystem.SimulatorScheme, StringComparison.OrdinalIgnoreCase) == true;

    public static void Apply<T>(AbstractValidator<T> v) where T : ISapSystemInput
    {
        v.RuleFor(c => c.Sid).NotEmpty().Length(3);
        v.RuleFor(c => c.Client).NotEmpty().Matches("^[0-9]{3}$");
        v.RuleFor(c => c.Host).NotEmpty().MaximumLength(200)
            .Must(h => IsSimulator(h) || !h.Contains("://", StringComparison.Ordinal)).WithMessage("Informe só o host (sem http:// ou https://).");
        v.RuleFor(c => c.AdtPort).InclusiveBetween(1, 65535).When(c => !IsSimulator(c.Host));
        v.RuleFor(c => c.Language).NotEmpty().Length(2);
        v.RuleFor(c => c.CertThumbprint).Matches("^[0-9A-Fa-f: ]{40,95}$").When(c => !string.IsNullOrWhiteSpace(c.CertThumbprint));
        v.RuleFor(c => c.ProjectType).IsInEnum();
        v.RuleFor(c => c.Author).NotEmpty().Matches("^(?i)AUT-[A-Z0-9]{1,16}$");
        v.RuleFor(c => c.DefaultRequest).MaximumLength(20);
        v.RuleFor(c => c.ExemptionApprover).MaximumLength(40);
        v.RuleFor(c => c.Username).NotEmpty().MaximumLength(40);
    }
}
