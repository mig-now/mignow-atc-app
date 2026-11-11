namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>Comparação de fontes do <c>aplicar_adt.ps1</c>: <c>Norm</c> = fim de linha único, sem espaços no fim de cada linha nem de linhas vazias no fim.</summary>
public static class SourceNormalizer
{
    public static string Normalize(string text) =>
        string.Join('\n', text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd().Split('\n').Select(l => l.TrimEnd()));

    public static bool SameSource(string a, string b) => string.Equals(Normalize(a), Normalize(b), StringComparison.Ordinal);
}
