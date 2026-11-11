namespace Mignow.Atc.Domain.Common;

internal static class Guard
{
    public static string Required(string? value, string code, int maxLength = 200)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new DomainException(code, $"{code}: valor obrigatório.");
        if (trimmed.Length > maxLength)
            throw new DomainException(code, $"{code}: máximo de {maxLength} caracteres.");
        return trimmed;
    }

    public static string? Optional(string? value, int maxLength = 500)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}
