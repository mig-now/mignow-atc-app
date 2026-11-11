using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Registry;

/// <summary>
/// Modelo de IA. A chave de API fica no cofre (<see cref="ApiKeySecretRef"/>). Um único padrão, usado em todas as chamadas
/// (decisão de 09.11.2026: mesmo modelo para todos os usos; padrão Claude Opus).
/// </summary>
public sealed class AiProvider : EntityBase
{
    private AiProvider() { }

    public string Name { get; private set; } = string.Empty;

    public AiProviderKind Kind { get; private set; }

    public string Model { get; private set; } = string.Empty;

    public string? ApiKeySecretRef { get; private set; }

    public bool IsDefault { get; private set; }

    public bool Active { get; private set; } = true;

    public static AiProvider Create(string name, AiProviderKind kind, string model)
    {
        var p = new AiProvider();
        p.Update(name, kind, model, true);
        return p;
    }

    public void Update(string name, AiProviderKind kind, string model, bool active)
    {
        Name = Guard.Required(name, "AiProvider.NameRequired", 120);
        Kind = kind;
        Model = Guard.Required(model, "AiProvider.ModelRequired", 120);
        Active = active;
        if (!active)
            IsDefault = false;
    }

    public void SetApiKeySecret(string secretRef) => ApiKeySecretRef = secretRef;

    public void MarkDefault(bool isDefault)
    {
        if (isDefault && !Active)
            throw new DomainException("AiProvider.InactiveCannotBeDefault", "Um provedor inativo não pode ser o padrão.");
        IsDefault = isDefault;
    }
}
