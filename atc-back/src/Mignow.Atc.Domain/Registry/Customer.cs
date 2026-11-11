using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Registry;

/// <summary>Cliente (equivale à pasta <c>clientes/&lt;CLIENTE&gt;/</c> das skills).</summary>
public sealed class Customer : EntityBase
{
    private Customer() { }

    public string Name { get; private set; } = string.Empty;

    public string? Code { get; private set; }

    public bool Active { get; private set; } = true;

    public static Customer Create(string name, string? code) =>
        new() { Name = Guard.Required(name, "Customer.NameRequired"), Code = Guard.Optional(code, 40)?.ToUpperInvariant() };

    public void Update(string name, string? code, bool active)
    {
        Name = Guard.Required(name, "Customer.NameRequired");
        Code = Guard.Optional(code, 40)?.ToUpperInvariant();
        Active = active;
    }
}
