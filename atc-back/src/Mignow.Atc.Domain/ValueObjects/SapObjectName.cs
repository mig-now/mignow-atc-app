namespace Mignow.Atc.Domain.ValueObjects;

/// <summary>Regras de nome do repositório SAP. Só pacotes do cliente (Z*/Y*, inclusive namespace /Z.../ e /Y.../) são alterados.</summary>
public static class SapObjectName
{
    public static bool IsCustomer(string? package)
    {
        var p = (package ?? string.Empty).Trim().ToUpperInvariant();
        if (p.Length == 0)
            return false;
        if (p.StartsWith('$'))
            return false; // pacote local nunca é transportado
        if (p.StartsWith('/'))
            p = p.TrimStart('/');
        return p[0] is 'Z' or 'Y';
    }

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();
}
