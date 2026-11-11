using System.Text;

namespace Mignow.Atc.Infra.Database.Context;

/// <summary>Convenção do blueprint: colunas MAIÚSCULAS_COM_SUBLINHADO (ex.: CreatedAt → CREATED_AT).</summary>
internal static class ColumnNames
{
    public static string Of(string property)
    {
        var sb = new StringBuilder(property.Length + 8);
        for (var i = 0; i < property.Length; i++)
        {
            var c = property[i];
            if (i > 0 && char.IsUpper(c) && (char.IsLower(property[i - 1]) || i + 1 < property.Length && char.IsLower(property[i + 1])))
                sb.Append('_');
            sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }
}
