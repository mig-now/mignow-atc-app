using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Endereços ADT do que o <c>aplicar_adt.ps1</c> deriva do endereço do fonte (objeto, contexto do include).</summary>
public static partial class SapUris
{
    /// <summary>
    /// Endereço do objeto: classe/interface sem o trecho depois do nome (<c>…/oo/classes/zcl_x/includes/implementations</c> → <c>…/oo/classes/zcl_x</c>);
    /// demais, o fonte sem <c>/source/main</c>.
    /// </summary>
    public static string ObjectUri(string sourceUri)
    {
        var path = sourceUri.Split('#', '?')[0];
        var m = OoRegex().Match(path);
        if (m.Success)
            return m.Groups[1].Value;
        return path.EndsWith("/source/main", StringComparison.OrdinalIgnoreCase) ? path[..^"/source/main".Length] : path;
    }

    /// <summary>
    /// Contexto da ativação de include (<c>?context=</c>): programa principal (<c>PROG</c>) ou grupo de funções (<c>FUGR</c>) do item;
    /// só quando o objeto é um include de programa. Vazio nos demais casos.
    /// </summary>
    public static string? ActivationContext(string objectUri, string objectType, string objectName)
    {
        if (!objectUri.Contains("/programs/includes/", StringComparison.OrdinalIgnoreCase))
            return null;
        var escaped = Uri.EscapeDataString(objectName.ToLowerInvariant());
        return objectType.ToUpperInvariant() switch
        {
            "PROG" => $"/sap/bc/adt/programs/programs/{escaped}",
            "FUGR" => $"/sap/bc/adt/functions/groups/{escaped}",
            _ => null,
        };
    }

    /// <summary>Nome do objeto no fim do endereço (decodificado, maiúsculas).</summary>
    public static string ObjectName(string objectUri) => Uri.UnescapeDataString(objectUri.TrimEnd('/').Split('/')[^1]).ToUpperInvariant();

    [GeneratedRegex(@"^(/sap/bc/adt/oo/(?:classes|interfaces)/[^/]+)/")]
    private static partial Regex OoRegex();
}
