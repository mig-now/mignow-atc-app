using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Regra da estrutura da tabela (<c>Campos</c> do <c>aplicar_ddic.ps1</c>): só tipo/anotação podem mudar; campos, ordem e chave não.</summary>
public static partial class DdicApplyRules
{
    /// <summary>Linhas de campo/include dentro do <c>define …</c>: <c>[KEY ]NOME</c> ou <c>INCLUDE X</c>.</summary>
    public static IReadOnlyList<string> Fields(string ddl)
    {
        List<string> fields = [];
        foreach (var line in SourceNormalizer.Normalize(ddl).Split('\n'))
        {
            if (FieldRegex().Match(line) is { Success: true } m)
                fields.Add($"{(m.Groups[1].Success ? "KEY " : string.Empty)}{m.Groups[2].Value.ToUpperInvariant()}");
            else if (IncludeRegex().Match(line) is { Success: true } i)
                fields.Add($"INCLUDE {i.Groups[1].Value.ToUpperInvariant()}");
        }
        return fields;
    }

    [GeneratedRegex(@"^\s*(key\s+)?([\w/]+)\s*:", RegexOptions.IgnoreCase)]
    private static partial Regex FieldRegex();

    [GeneratedRegex(@"^\s*include\s+([\w/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex IncludeRegex();
}
